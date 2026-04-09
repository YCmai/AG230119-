using AciModule.Domain.Shared;
using SuperPortLibrary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace AciModule.Domain
{
   public class AciConnection
    {
        private bool _Disposed = false;
        private bool _UseLocalIP = true;

        public bool Disposed { get { return _Disposed; } private set { _Disposed = value; } }
        public bool UseLocalIP { get { return _UseLocalIP; } }                          //使用本地IP
        public CommTCPClient TCPClient { get { return _TCPClient; } }
        public AciProtocol Protocol { get { return _Protocol; } }
      
        public bool Connected { get { return TCPClient.Connected; } }                   //是否连接
        public bool Communicating { get { return TCPClient.Communicating; } }           //连接中
        public bool DataRecord { get { return _DataRecord; } }                          //数据记录
        public bool ConnectionRecord { get { return _ConnectionRecord; } }              //连接记录 
        public IPAddress ServerIP { get { return TCPClient.ServerIP; } }                //保存IP
        public int ServerPort { get { return TCPClient.ServerPort; } }                  //保存端口
        public IPEndPoint LocalEndPoint { get { return TCPClient.LocalEndPoint; } }     //本地端点
        public IPEndPoint RemoteEndPoint { get { return TCPClient.RemoteEndPoint; } }   //远程端点

        public event EventHandler ConnectedChanged;
        public event EventHandler OmPlcToHost;
        public event AciDataEventHandler DataTransmited, DataReceived;
        public event AciDataEventHandler RequestDataReceived, ErrorDataReceived;

        public AciConnection()
        {
            InitialDataLog();
            InitialTCPClient();
        }
        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                DisposeDataLog();
                DisposeTCPClient();
            }
        }

        #region DataLog 
     //   private SQLiteBase _DataLog;
        private bool _DataRecord = false;
        private bool _ConnectionRecord = true;
        private ConcurrentQueue<string> DataLogQueue;
        private bool DataLogRunning = false;

        private string ReceivedTableName = "ReceivedData";
        private string TransmitedTableName = "TransmitedData";
        private string ConnectionTableName = "ConnectionData";

        public void SetDataRecord(bool valid)
        {
            if (DataRecord != valid)
            {
                _DataRecord = valid;
            }
        }

        public void SetConnectionRecord(bool valid)
        {
            if (ConnectionRecord != valid)
            {
                _ConnectionRecord = valid;
            }
        }

        private void InitialDataLog()
        {  
            DataLogQueue = new ConcurrentQueue<string>();
        }

        private void DisposeDataLog()
        {
            DataLogRunning = false;
        }

        
        /// <summary>
        /// 插入收到日志
        /// </summary>
        /// <param name="data"></param>
        private void InsertReceivedLog(AciProtocolData data)
        {
            if (!DataRecord) return;

            byte[] bytedata = data.BytesData;
            GeneralAciData acidata = data.AciData;

            string datastring = "x'";
            if (bytedata != null && bytedata.Length > 0)
            {
                foreach (byte b in bytedata)
                {
                    datastring += b.ToString("X2");
                }
            }
            datastring += "'";

            string sql = string.Format("INSERT INTO {0} (Function, Type, ErrorCode, Data) VALUES ({1}, {2}, {3}, {4})", ReceivedTableName, (int)acidata.DataFunction, (int)acidata.DataType, acidata.CompileErrorCode, datastring);

            DataLogQueue.Enqueue(sql);
        }
        /// <summary>
        /// 插入传输日志
        /// </summary>
        /// <param name="data"></param>
        private void InsertTransmitedLog(AciProtocolData data)
        {
            if (!DataRecord) return;

            byte[] bytedata = data.BytesData;
            GeneralAciData acidata = data.AciData;

            string datastring = "x'";
            if (bytedata != null && bytedata.Length > 0)
            {
                foreach (byte b in bytedata)
                {
                    datastring += b.ToString("X2");
                }
            }
            datastring += "'";

            string sql = string.Format("INSERT INTO {0} (Function, Type, ErrorCode, Data) VALUES ({1}, {2}, {3}, {4})", TransmitedTableName, (int)acidata.DataFunction, (int)acidata.DataType, acidata.CompileErrorCode, datastring);

            DataLogQueue.Enqueue(sql);
        }
        /// <summary>
        /// 插入连接日志
        /// </summary>
        private void InsertConnectionLog()
        {
            string sql = string.Format("INSERT INTO {0} (Connected, LocalIP, LocalPort, RemoteIP, RemotePort) Values ({1}, '{2}', '{3}', '{4}', '{5}')", ConnectionTableName, TCPClient.Connected ? 1 : 0, TCPClient.LocalIPAddress, TCPClient.LocalPort, TCPClient.RemoteIPAddress, TCPClient.RemotePort);

            DataLogQueue.Enqueue(sql);
        }
        #endregion

        #region TCPClient
        private int MagicCodeSeek = 0;
        private CommTCPClient _TCPClient;
        private AciProtocol _Protocol;
        private bool WaitingAcknowledge = false;
        private bool RequestDataHandling = false;
        private bool CommandHandling = false;

        private ConcurrentQueue<AciProtocolData> _AcknowledgeQueue;
        private ConcurrentQueue<AciProtocolData> _RequestQueue;
        private ConcurrentQueue<AciProtocolData> _ErrorQueue;
        private ConcurrentQueue<AciCommandData> _CommandQueue;

        /// <summary>
        /// 确认队列
        /// </summary>
        private ConcurrentQueue<AciProtocolData> AcknowledgeQueue { get { return _AcknowledgeQueue; } }
        /// <summary>
        /// 接收队列
        /// </summary>
        private ConcurrentQueue<AciProtocolData> RequestQueue { get { return _RequestQueue; } }
        /// <summary>
        /// 错误队列
        /// </summary>
        private ConcurrentQueue<AciProtocolData> ErrorQueue { get { return _ErrorQueue; } }
        /// <summary>
        /// 命令队列
        /// </summary>
        private ConcurrentQueue<AciCommandData> CommandQueue { get { return _CommandQueue; } }

        public void SetServerLocalIP(int port)
        {
            _UseLocalIP = true;
            string hostName = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostName);

            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    TCPClient.SetServerEndPoint(ip, port);
                }
            }
        }

        public void SetServerEndPoint(IPAddress ip, int port)
        {
            _UseLocalIP = false;
            TCPClient.SetServerEndPoint(ip, port);
        }

        #region SendAciCommand
        public AciCommandData SendOrderInitial(AciCommandCallBack callback, int key, int trp, int pri, int[] vals)
        {
            OrderInitiateAciData order = new OrderInitiateAciData()
            {
                OrderKey = key,
                StructureID = trp,
                OrderPriority = pri,
                ParamValues = vals
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendOrderRequestAll(AciCommandCallBack callback, int itemo, int itemn, uint lpflag)
        {
            OrderRequestAciData order = new OrderRequestAciData()
            {
                MagicIndex = GetMagicCode(),
                ItemCode = OrderItemCode.NumericalInterval,
                ItemOffset = itemo,
                ItemNumber = itemn,
                ParamFlag = lpflag
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendOrderRequestUsed(AciCommandCallBack callback, int itemn, uint lpflag)
        {
            OrderRequestAciData order = new OrderRequestAciData()
            {
                MagicIndex = GetMagicCode(),
                ItemCode = OrderItemCode.UsedIndex,
                ItemOffset = 0,
                ItemNumber = itemn,
                ParamFlag = lpflag
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendOrderDeleteViaOrder(AciCommandCallBack callback, int index)
        {
            OrderDeleteAciData order = new OrderDeleteAciData()
            {
                OrderIndex = index
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendOrderDeleteViaCarrier(AciCommandCallBack callback, int index)
        {
            OrderDeleteAciData order = new OrderDeleteAciData()
            {
                CarrierID = index
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendLocalParamInsert(AciCommandCallBack callback, int oix, int pix, int[] pvals)
        {
            LocalParamCommandAciData order = new LocalParamCommandAciData()
            {
                OrderIndex = oix,
                Command = LocalParamCommandCode.InsertSpontaneous,
                ParamIndex = pix,
                ParamValues = pvals
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendLocalParamDelete(AciCommandCallBack callback, int oix, int pix)
        {
            LocalParamCommandAciData order = new LocalParamCommandAciData()
            {
                OrderIndex = oix,
                Command = LocalParamCommandCode.Delete,
                ParamIndex = pix,
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendLocalParamRead(AciCommandCallBack callback, int oix, int pix, int[] pvals)
        {
            LocalParamCommandAciData order = new LocalParamCommandAciData()
            {
                OrderIndex = oix,
                Command = LocalParamCommandCode.Read,
                ParamIndex = pix,
                ParamValues = pvals
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendOrderPriority(AciCommandCallBack callback, int oix, int pri)
        {
            LocalParamCommandAciData order = new LocalParamCommandAciData()
            {
                OrderIndex = oix,
                Command = LocalParamCommandCode.ChangePriority,
                OrderPriority = pri
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendConnectCarrier(AciCommandCallBack callback, int oix, int cid)
        {
            LocalParamCommandAciData order = new LocalParamCommandAciData()
            {
                OrderIndex = oix,
                Command = LocalParamCommandCode.ConnectVehicle,
                CarrierID = cid
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendGlobalParamRead(AciCommandCallBack callback, int index, int number)
        {
            GlobalParamCommandAciData order = new GlobalParamCommandAciData()
            {
                MagicIndex = GetMagicCode(),
                Command = GlobalParamCommandCode.Read,
                ParamIndex = index,
                ParamNumber = number
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }

        public AciCommandData SendGlobalParamWrite(AciCommandCallBack callback, int index, int number, int[] vals)
        {
            GlobalParamCommandAciData order = new GlobalParamCommandAciData()
            {
                MagicIndex = GetMagicCode(),
                Command = GlobalParamCommandCode.Write,
                ParamIndex = index,
                ParamNumber = number,
                ParamValues = vals
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }
       
        public AciCommandData SendHostToAgvOmPlcRead(AciCommandCallBack callback, int index, int carId, int magic, int num)
        {
            OmPlcCommandAciData order = new OmPlcCommandAciData
            {
                MagicIndex = magic,
                Command = OmPlcCommandCode.ReadMultiByte,           //c 功能码
                ParamCarId = carId,                                 //AGVID
                ParamIndex = index,                                   //plc偏移量
                ParamNumber = num,                                  //读取的总字节数
            };
            AciCommandData data = new AciCommandData(order, callback);
            CommandQueue.Enqueue(data);
            return data;
        }
        public AciCommandData SendOmPlcToAgvWrite(AciCommandCallBack callBack, int index, int carId, int magic, int num, int[] vals)
        {
            OmPlcCommandAciData order = new OmPlcCommandAciData
            {
                MagicIndex = magic,
                Command = OmPlcCommandCode.WriteMultiByte,
                ParamIndex = index,
                ParamCarId = carId,
                ParamNumber = num,
                ParamValues = vals,
            };
            AciCommandData data = new AciCommandData(order, callBack);
            CommandQueue.Enqueue(data);
            return data;
        }
        #endregion

        private void InitialTCPClient()
        {
            _AcknowledgeQueue = new ConcurrentQueue<AciProtocolData>();
            _RequestQueue = new ConcurrentQueue<AciProtocolData>();
            _ErrorQueue = new ConcurrentQueue<AciProtocolData>();
            _CommandQueue = new ConcurrentQueue<AciCommandData>();

            _Protocol = new AciProtocol();
            _TCPClient = new CommTCPClient();

            TCPClient.SetProtocol(Protocol);

            InitialEvent();
            DataHandlerStart();
            CommandHandlerStart();
        }

        private void DisposeTCPClient()
        {
            RequestDataHandling = false;
            CommandHandling = false;
            DisposeEvent();

            TCPClient.Dispose();
        }

        private void InitialEvent()
        {
            TCPClient.ConnectionChanged += new CommunicationEventHandler(TCPClient_ConnectionChanged);
            TCPClient.DataReceived += new ProtocolDataEventHandler(TCPClient_DataReceived);
            TCPClient.DataTransmited += new ProtocolDataEventHandler(TCPClient_DataTransmited);
        }

        private void DisposeEvent()
        {
            TCPClient.ConnectionChanged -= TCPClient_ConnectionChanged;
            TCPClient.DataReceived -= TCPClient_DataReceived;
            TCPClient.DataTransmited -= TCPClient_DataTransmited;
        }

        private void DataHandlerStart()
        {
            Thread thread = new Thread(() =>
            {
                while (RequestDataHandling)
                {
                    while (!RequestQueue.IsEmpty || !ErrorQueue.IsEmpty)
                    {
                        AciProtocolData data;

                        if (!RequestQueue.IsEmpty && RequestQueue.TryDequeue(out data))
                        {
                            //通知事件
                            if (RequestDataReceived != null)
                            {
                                RequestDataReceived(this, new AciDataEventArgs(data.AciData, data.BytesData));
                            }
                        }

                        if (!ErrorQueue.IsEmpty && ErrorQueue.TryDequeue(out data))
                        {
                            if (ErrorDataReceived != null)
                            {
                                ErrorDataReceived(this, new AciDataEventArgs(data.AciData, data.BytesData));
                            }
                        }

                        Thread.Sleep(10);
                    }
                    Thread.Sleep(200);
                }
            });
            RequestDataHandling = true;
            thread.IsBackground = true;
            thread.Start();
        }

        private void CommandHandlerStart()
        {
            Thread thread = new Thread(ComandHandler);
            CommandHandling = true;
            thread.IsBackground = true;
            thread.Start();
        }

        private void ComandHandler()
        {
            while (CommandHandling)
            {
                while (!CommandQueue.IsEmpty)
                {
                    AciCommandData data;

                    if (CommandQueue.TryDequeue(out data))
                    {
                        DateTime start = DateTime.Now;
                        WaitingAcknowledge = true;
                        
                        TransmitData(data.RequestData);//处理发送数据
                        int CheckDelay = 5;

                        if (data.RequestData.DecompileErrorCode > 0)
                        {
                            data.Cancel(AciCommandErrorCode.DataError);
                        }
                        else
                        {
                            switch (data.RequestData.DataType)
                            {
                                case MessageType.OrderInitiate:
                                    {
                                        OrderInitiateAciData request = data.RequestData as OrderInitiateAciData;

                                        while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 300)
                                        {
                                            Thread.Sleep(CheckDelay);

                                            while (!AcknowledgeQueue.IsEmpty)
                                            {
                                                AciProtocolData ackdata;
                                                AcknowledgeQueue.TryDequeue(out ackdata);

                                                if (ackdata.AciData.DataType == MessageType.OrderAcknowledge)
                                                {
                                                    OrderAcknowledgeAciData acknowledge = ackdata.AciData as OrderAcknowledgeAciData;
                                                    if (acknowledge.OrderKey == request.OrderKey)
                                                    {
                                                        data.AddAcknowledge(ackdata.AciData);
                                                        data.SetAcknowledged();
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MessageType.OrderRequest:
                                    {
                                        OrderRequestAciData request = data.RequestData as OrderRequestAciData;

                                        while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 1000)
                                        {
                                            Thread.Sleep(CheckDelay);

                                            while (!AcknowledgeQueue.IsEmpty)
                                            {
                                                AciProtocolData ackdata;
                                                AcknowledgeQueue.TryDequeue(out ackdata);

                                                start = DateTime.Now;
                                                if (ackdata.AciData.DataType == MessageType.OrderStatus)
                                                {
                                                    OrderStatusAciData status = ackdata.AciData as OrderStatusAciData;
                                                    if (status.MagicIndex == request.MagicIndex)
                                                    {
                                                        data.AddAcknowledge(ackdata.AciData);

                                                        if (status.ErrorCode != OrderErrorCode.None)
                                                        {
                                                            data.SetAcknowledged();
                                                        }
                                                    }
                                                }
                                                else if (ackdata.AciData.DataType == MessageType.LocalParamContents)
                                                {
                                                    data.AddAcknowledge(ackdata.AciData);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MessageType.OrderDelete:
                                    {
                                        OrderDeleteAciData request = data.RequestData as OrderDeleteAciData;

                                        while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 300)
                                        {
                                            Thread.Sleep(CheckDelay);

                                            while (!AcknowledgeQueue.IsEmpty)
                                            {
                                                AciProtocolData ackdata;
                                                AcknowledgeQueue.TryDequeue(out ackdata);

                                                if (ackdata.AciData.DataType == MessageType.OrderAcknowledge)
                                                {
                                                    OrderAcknowledgeAciData acknowledge = ackdata.AciData as OrderAcknowledgeAciData;
                                                    if (acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupII))
                                                    {
                                                        data.AddAcknowledge(ackdata.AciData);
                                                        data.SetAcknowledged();
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MessageType.LocalParamCommand:
                                    {
                                        LocalParamCommandAciData request = data.RequestData as LocalParamCommandAciData;

                                        while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 600)
                                        {
                                            Thread.Sleep(CheckDelay);

                                            while (!AcknowledgeQueue.IsEmpty)
                                            {
                                                AciProtocolData ackdata;
                                                AcknowledgeQueue.TryDequeue(out ackdata);

                                                switch (request.Command)
                                                {
                                                    case LocalParamCommandCode.InsertSpontaneous:
                                                    case LocalParamCommandCode.InsertRequested:
                                                    case LocalParamCommandCode.Delete:
                                                        {
                                                            if (ackdata.AciData.DataType == MessageType.OrderAcknowledge)
                                                            {
                                                                OrderAcknowledgeAciData acknowledge = ackdata.AciData as OrderAcknowledgeAciData;
                                                                if (request.OrderIndex == acknowledge.OrderIndex &&
                                                                    acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupIII))
                                                                {
                                                                    data.AddAcknowledge(ackdata.AciData);
                                                                    data.SetAcknowledged();
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case LocalParamCommandCode.Read:
                                                        {
                                                            if (ackdata.AciData.DataType == MessageType.LocalParamContents)
                                                            {
                                                                LocalParamContentsAciData acknowledge = ackdata.AciData as LocalParamContentsAciData;

                                                                if (request.OrderIndex == acknowledge.OrderIndex)
                                                                {
                                                                    data.AddAcknowledge(ackdata.AciData);
                                                                    data.SetAcknowledged();
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case LocalParamCommandCode.ChangePriority:
                                                        {
                                                            if (ackdata.AciData.DataType == MessageType.OrderAcknowledge)
                                                            {
                                                                OrderAcknowledgeAciData acknowledge = ackdata.AciData as OrderAcknowledgeAciData;
                                                                if (request.OrderIndex == acknowledge.OrderIndex &&
                                                                    acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupVII))
                                                                {
                                                                    data.AddAcknowledge(ackdata.AciData);
                                                                    data.SetAcknowledged();
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    case LocalParamCommandCode.ConnectVehicle:
                                                        {
                                                            if (ackdata.AciData.DataType == MessageType.OrderAcknowledge)
                                                            {
                                                                OrderAcknowledgeAciData acknowledge = ackdata.AciData as OrderAcknowledgeAciData;
                                                                if (request.OrderIndex == acknowledge.OrderIndex &&
                                                                    acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupVIII))
                                                                {
                                                                    data.AddAcknowledge(ackdata.AciData);
                                                                    data.SetAcknowledged();
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MessageType.GlobalParamCommand:
                                    {
                                        GlobalParamCommandAciData request = data.RequestData as GlobalParamCommandAciData;

                                        while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 300)
                                        {
                                            Thread.Sleep(CheckDelay);

                                            while (!AcknowledgeQueue.IsEmpty)
                                            {
                                                AciProtocolData ackdata;
                                                AcknowledgeQueue.TryDequeue(out ackdata);

                                                if (ackdata.AciData.DataType == MessageType.GlobalParamStatus)
                                                {
                                                    GlobalParamStatusAciData acknowledge = ackdata.AciData as GlobalParamStatusAciData;
                                                    if (acknowledge.MagicIndex == request.MagicIndex)
                                                    {
                                                        data.AddAcknowledge(ackdata.AciData);
                                                        data.SetAcknowledged();
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MessageType.HostToAgvOmPlc:
                                    OmPlcCommandAciData request1 = data.RequestData as OmPlcCommandAciData;
                                    while (!data.Acknowledged && (DateTime.Now - start).TotalMilliseconds < 300)
                                    {
                                        Thread.Sleep(CheckDelay);
                                        while (!AcknowledgeQueue.IsEmpty)
                                        {
                                            AciProtocolData ackdata;
                                            AcknowledgeQueue.TryDequeue(out ackdata);
                                            if (ackdata.AciData.DataType == MessageType.GlobalParamStatus)
                                            {
                                                GlobalParamStatusAciData acknowledge = ackdata.AciData as GlobalParamStatusAciData;
                                                if (acknowledge.MagicIndex == request1.MagicIndex)
                                                {
                                                    data.AddAcknowledge(ackdata.AciData);
                                                    data.SetAcknowledged();
                                                    break;
                                                }
                                            }

                                            if (ackdata.AciData.DataType == MessageType.HostToAgvOmPlc)
                                            {
                                                OmPlcStatusAciData acknowledge = ackdata.AciData as OmPlcStatusAciData;
                                                if (acknowledge.MagicIndex == request1.MagicIndex)
                                                {
                                                    data.AddAcknowledge(ackdata.AciData);
                                                    data.SetAcknowledged();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        WaitingAcknowledge = false;

                        if (!data.Acknowledged)
                        {
                            data.Cancel(AciCommandErrorCode.TimeOut);
                        }

                        data.CallBack();
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(200);
            }
        }

        private int GetMagicCode()
        {
            if (MagicCodeSeek++ >= 0x7FFF) MagicCodeSeek = 0;
            return MagicCodeSeek;
        }

        private int GetMagicOmPlcCode()
        {
            if (MagicCodeSeek++ >= 0xE001) MagicCodeSeek = 0;
            return MagicCodeSeek;
        }
        private void TransmitData(GeneralAciData data)
        {
            TCPClient.DataTransmit(data);
        }

        private void TCPClient_DataReceived(object sender, ProtocolDataEventArgs e)
        {
            if (e.Data is AciProtocolData)
            {
                AciProtocolData pdata = e.Data as AciProtocolData;
                GeneralAciData data = pdata.AciData;

                InsertReceivedLog(pdata);//TCp客户端接收数据过来，这个是插入接收数据

                if (data.CompileErrorCode > 0)
                {
                    ErrorQueue.Enqueue(pdata);
                }
                else
                {

                    switch (data.DataFunction)
                    {
                        case FunctionCode.Normal:
                            switch (data.DataType)
                            {
                                case MessageType.OrderEvent:
                                case MessageType.LocalParamRequest:
                                    RequestQueue.Enqueue(pdata);
                                    break;
                                case MessageType.AgvOmPlcToHost:
                                    OmPlcStatusAciData plcAciData = data as OmPlcStatusAciData;
                                    OmPlcToHostParam param = new OmPlcToHostParam()
                                    {
                                        Status = plcAciData.Status,
                                        ParamCarID = plcAciData.ParamCarID,
                                        ParamValues = plcAciData.ParamValues
                                    };
                                    if (plcAciData != null)
                                    {
                                        if (OmPlcToHost != null)
                                        {
                                            OmPlcToHost(this, param);
                                        }
                                    }

                                    break;
                                case MessageType.HostToAgvOmPlc:
                                    break;
                                case MessageType.OrderStatus:
                                case MessageType.LocalParamContents:
                                case MessageType.GlobalParamStatus:
                                    if (WaitingAcknowledge)
                                    {
                                        AcknowledgeQueue.Enqueue(pdata);
                                        if (pdata.GlobalParamValues == null) break;
                                        if (pdata.GlobalParamValues.Length == 10)
                                        {
                                            
                                        }
                                    }
                                    break;
                                case MessageType.OrderAcknowledge:
                                    {
                                        OrderAcknowledgeAciData acknowledge = data as OrderAcknowledgeAciData;
                                        if (acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupIV)
                                            || acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupV)
                                            || acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupVI)
                                            || acknowledge.AcknowledgeGroup.HasFlag(AckGroupCode.GroupIX))
                                        {
                                            RequestQueue.Enqueue(pdata);
                                        }
                                        else
                                        {
                                            AcknowledgeQueue.Enqueue(pdata);
                                        }
                                    }
                                    break;
                                default:
                                    ErrorQueue.Enqueue(pdata);
                                    break;
                            }
                            break;
                        case FunctionCode.HeartBeatPoll:
                            {
                                TransmitData(new GeneralAciData()
                                {
                                    DataFunction = FunctionCode.HeartBeatACK,
                                    DataType = MessageType.None
                                });
                            }
                            break;
                        default:
                            ErrorQueue.Enqueue(pdata);
                            break;
                    }
                }

                if (DataReceived != null) { DataReceived(this, new AciDataEventArgs(data, pdata.BytesData)); }
            }
        }

        private void TCPClient_DataTransmited(object sender, ProtocolDataEventArgs e)
        {
            if (e.Data is AciProtocolData)
            {
                AciProtocolData data = e.Data as AciProtocolData;
                InsertTransmitedLog(data); //记录已经发送出去的数据

                if (DataTransmited != null)
                {
                    DataTransmited(this, new AciDataEventArgs(data.AciData, data.BytesData));
                }
            }
        }

        private void TCPClient_ConnectionChanged(object sender, CommunicationEventArgs e)
        {
            InsertConnectionLog(); //记录连接的主机ip和端口

            if (!TCPClient.Connected)
            {
                if (UseLocalIP)
                {
                    string hostName = Dns.GetHostName();
                    IPAddress[] ips = Dns.GetHostAddresses(hostName);
                    foreach (IPAddress ip in ips)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            TCPClient.SetServerEndPoint(ip, ServerPort);
                        }
                    }
                }
            }

            if (ConnectedChanged != null) ConnectedChanged(this, new EventArgs());
        }
        #endregion
    }

    public delegate void AciDataEventHandler(object sender, AciDataEventArgs e);

    public delegate void AciCommandCallBack(AciCommandData data);

    class OmPlcToHostParam : EventArgs
    {
        public OmPlcCommandCode Status { get; set; }
        public int ParamCarID { get; set; }
        public int[] ParamValues { get; set; }
    }

    public class AciDataEventArgs : EventArgs
    {
        public GeneralAciData AciData { get; private set; }
        public byte[] ByteData { get; private set; }
        public FunctionCode DataFunction { get { return AciData.DataFunction; } }
        public MessageType DataType { get { return AciData.DataType; } }

        public AciDataEventArgs(GeneralAciData acidata, byte[] bytedata)
        {
            AciData = acidata;
            ByteData = bytedata;
        }
    }
}
