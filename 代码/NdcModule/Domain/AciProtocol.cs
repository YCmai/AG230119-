
using AciModule.Domain.Shared;
using SuperPortLibrary;

namespace AciModule.Domain
{
    /// <summary>
    /// Aci协议，Protocol这个协议是封装好的dll
    /// </summary>
    public class AciProtocol : Protocol
    {
        //包头长度
        private int HeaderLength = 2;
        //包头
        private byte[] HeaderCode = new byte[] { 0x87, 0xCD };//135,205

        private List<ReadBuffer> tmpBuffers = new List<ReadBuffer>();

        protected override ProtocolData OnAutoWriteData()
        {
            return null;
        }

        protected override ProtocolData OnErrorWriteData()
        {
            return null;
        }

        protected override ProtocolData OnFailWriteData()
        {
            return null;
        }

        protected override ProtocolData OnCompileData(byte data)
        {
            ProtocolData output = null;

            List<ReadBuffer> RemoveBuffers = new List<ReadBuffer>();
            foreach (ReadBuffer tmpbuffer in tmpBuffers)
            {
                switch (tmpbuffer.State)
                {
                    case 0://Header code
                        if (data == HeaderCode[tmpbuffer.HeadPoint])
                        {
                            tmpbuffer.HeadPoint++;
                            if (tmpbuffer.HeadPoint >= HeaderLength)
                            {
                                tmpbuffer.State = 1;
                            }
                        }
                        else
                        {
                            RemoveBuffers.Add(tmpbuffer);
                        }
                        break;
                    case 1://Size of header 1
                        {
                            tmpbuffer.HeaderSize += (int)data * 0x100;
                            tmpbuffer.State = 2;
                        }
                        break;
                    case 2://Size of header 2
                        {
                            tmpbuffer.HeaderSize += (int)data;

                            if (tmpbuffer.HeaderSize == HeaderLength * 4)
                            {
                                tmpbuffer.State = 3;
                            }
                        }
                        break;
                    case 3: //Size of Message 1
                        {
                            tmpbuffer.MessageSize += (int)data * 0x100;
                            tmpbuffer.State = 4;
                        }
                        break;
                    case 4: //Size of Message 2
                        {
                            tmpbuffer.MessageSize += (int)data;
                            tmpbuffer.MessageSize += 2; //Add FunctionCode Length
                            tmpbuffer.State = 5;
                        }
                        break;
                    case 5: //Receive data
                        {
                            tmpbuffer.Buff.Add(data);
                            if (tmpbuffer.Buff.Count >= tmpbuffer.MessageSize)
                            {
                                byte[] tmp = tmpbuffer.Buff.ToArray();

                                output = new AciProtocolData(tmp);
                                RemoveBuffers.Add(tmpbuffer);
                            }
                        }
                        break;
                    default:
                        {
                            RemoveBuffers.Add(tmpbuffer);
                        }
                        break;
                }
            }

            foreach (ReadBuffer tmpbyte in RemoveBuffers)
            {
                tmpBuffers.Remove(tmpbyte);
            }

            if (data == HeaderCode[0])
            {
                tmpBuffers.Add(new ReadBuffer());
            }

            return output;
        }

        protected override void OnReadTick()
        {

        }

        protected override ProtocolData OnDecompileData(ApplicationData data)
        {
            ProtocolData output = new AciProtocolData(data);
            return output;
        }

        protected override byte[] OnTransmitData(ProtocolData data)
        {
            if (data.BytesLength > 0)
            {
                int oriLength = data.BytesData.Length;
                byte[] oriBytes = data.BytesData;
                byte[] newBytes = new byte[oriLength + HeaderLength + 4];
                int HeaderSize = HeaderLength * 4;

                Buffer.BlockCopy(HeaderCode, 0, newBytes, 0, HeaderLength);
                DataTransform.UInt16ToBytes(HeaderSize, newBytes, HeaderLength);
                DataTransform.UInt16ToBytes(oriLength, newBytes, HeaderLength + 2);
                Buffer.BlockCopy(oriBytes, 0, newBytes, HeaderLength + 4, oriLength);

                return newBytes;
            }


            return data.BytesData;
        }

        protected override void OnWriteTick()
        {

        }
        /// <summary>
        /// 读缓冲区
        /// </summary>
        private class ReadBuffer
        {
            /// <summary>
            /// 状态
            /// </summary>
            public int State { get; set; }
            /// <summary>
            /// 缓存集合
            /// </summary>
            public List<byte> Buff { get; set; }
            /// <summary>
            /// 当前长度
            /// </summary>
            public int CurrentLength { get; set; }
            /// <summary>
            /// 顶点或者头点
            /// </summary>
            public int HeadPoint { get; set; }
            /// <summary>
            /// 包头大小
            /// </summary>
            public int HeaderSize { get; set; }
            /// <summary>
            /// 数据大小
            /// </summary>
            public int MessageSize { get; set; }

            public ReadBuffer()
            {
                State = 0;

                Buff = new List<byte>();
                CurrentLength = -1;
                HeadPoint = 1;
                HeaderSize = 0;
                MessageSize = 0;
            }
        }
    }

    class AciProtocolData : ProtocolData
    {
        public GeneralAciData AciData { get { return ApplicationData as GeneralAciData; } }
        public int[] GlobalParamValues { get; set; }
        public int ParamIndex { get; set; }
        public AciProtocolData(ApplicationData data)
            : base(data)
        {

        }

        public AciProtocolData(byte[] data)
            : base(data, true)
        {

        }

        protected override void OnCompileData()
        {
            GeneralAciData data = null;
            int error = 0;

            FunctionCode func = FunctionCode.Error;
            MessageType mType = MessageType.None;
            if (BytesLength >= 2)
            {
                func = (FunctionCode)DataTransform.BytesToUInt16(BytesData, 0);
                if (func == FunctionCode.Normal)
                {
                    if (BytesLength >= 6)
                    {
                        int StaticLength = 6;

                        mType = (MessageType)DataTransform.BytesToUInt16(BytesData, 2);
                        int mSize = DataTransform.BytesToUInt16(BytesData, 6);

                        switch (mType)
                        {
                            case MessageType.OrderAcknowledge: //订单确认
                                {
                                    int index = 0, trp = 0, par = 0, key = -1;
                                    AckStatusCode status = AckStatusCode.None;
                                    AckGroupCode group = AckGroupCode.None;

                                    if (BytesLength < StaticLength + 6) { error = 3; break; }

                                    index = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    if (index == 0xFFFF) index = -1;
                                    trp = DataTransform.BytesToUInt8(BytesData, StaticLength + 2);
                                    status = (AckStatusCode)DataTransform.BytesToUInt8(BytesData, StaticLength + 3);
                                    par = DataTransform.BytesToUInt8(BytesData, StaticLength + 4);

                                    if (!Enum.IsDefined(typeof(AckStatusCode), status)) { error = 10; break; }

                                    if (BytesLength > StaticLength + 7)
                                    {
                                        key = DataTransform.BytesToUInt16(BytesData, StaticLength + 6);
                                    }

                                    switch (status)
                                    {
                                        case AckStatusCode.OrderRejected:
                                            // |= 的意思是group是句子集合AckGroupCode的模型 ，|是按位或，&是按位与
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.OrderAccepted:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.ParamNumberInvalid:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.PriorityError: group |= AckGroupCode.GroupI | AckGroupCode.GroupVII; break;
                                        case AckStatusCode.StructureError:
                                            group |= AckGroupCode.GroupI | AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.FullBuffer:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.ParamNumberHigh:
                                            group |= AckGroupCode.GroupI | AckGroupCode.GroupII | AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.UpdateNotAllow:
                                            group |= AckGroupCode.GroupI | AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.MissingParam:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.DuplicatedKey:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.FormatCodeInvalid:
                                            group |= AckGroupCode.GroupI;
                                            break;
                                        case AckStatusCode.CancelAcknowledge:
                                            group |= AckGroupCode.GroupII;
                                            break;
                                        case AckStatusCode.OrderCancelled:
                                            group |= AckGroupCode.GroupII;
                                            break;
                                        case AckStatusCode.IndexNotActivated:
                                            group |= AckGroupCode.GroupII | AckGroupCode.GroupIII | AckGroupCode.GroupVII | AckGroupCode.GroupVIII;
                                            break;
                                        case AckStatusCode.CarrierError:
                                            group |= AckGroupCode.GroupII;
                                            break;
                                        case AckStatusCode.IndexInvalid:
                                            group |= AckGroupCode.GroupII | AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.ParamAcknowledge:
                                            group |= AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.ParamDeleted:
                                            group |= AckGroupCode.GroupIII;
                                            break;
                                        case AckStatusCode.OrderFinished:
                                            group |= AckGroupCode.GroupIV;
                                            break;
                                        case AckStatusCode.ParamAccepted:
                                            group |= AckGroupCode.GroupV;
                                            break;
                                        case AckStatusCode.ParamRejected:
                                            group |= AckGroupCode.GroupV;
                                            break;
                                        case AckStatusCode.InputReleased:
                                            group |= AckGroupCode.GroupV;
                                            break;
                                        case AckStatusCode.FatalError:
                                            group |= AckGroupCode.GroupVI;
                                            break;
                                        case AckStatusCode.PriorityAcknowledge:
                                            group |= AckGroupCode.GroupVII;
                                            break;
                                        case AckStatusCode.ConnectionSucceed:
                                            group |= AckGroupCode.GroupVIII;
                                            break;
                                        case AckStatusCode.ConnectionFail:
                                            group |= AckGroupCode.GroupVIII;
                                            break;
                                        case AckStatusCode.AllocationCarrier:
                                            group |= AckGroupCode.GroupIX;
                                            break;
                                        case AckStatusCode.LostCarrier:
                                            group |= AckGroupCode.GroupIX;
                                            break;
                                        case AckStatusCode.ConnectionCarrier:
                                            group |= AckGroupCode.GroupIX;
                                            break;
                                        default: break;
                                    }

                                    data = new OrderAcknowledgeAciData()
                                    {
                                        OrderIndex = index,
                                        OrderKey = key,
                                        StructureID = trp,
                                        AcknowledgeStatus = status,
                                        AcknowledgeGroup = group,
                                        AcknowledgeParam = par
                                    };
                                }
                                break;
                            case MessageType.OrderEvent://订单事件
                                {
                                    int index = -1, trp = 0, m1 = -1, m2 = -1, m3 = -1, cid = 0, cstate = 0, cstn = 0, lpno = 0;
                                    int[] lps = new int[] { };
                                    EventStatusCode status = EventStatusCode.NotUsed;

                                    if (BytesLength < StaticLength + 14) { error = 4; break; }

                                    index = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    trp = DataTransform.BytesToUInt8(BytesData, StaticLength + 2);
                                    status = (EventStatusCode)DataTransform.BytesToUInt8(BytesData, StaticLength + 3);
                                    m1 = DataTransform.BytesToUInt16(BytesData, StaticLength + 4);
                                    m2 = DataTransform.BytesToUInt16(BytesData, StaticLength + 6);
                                    cid = DataTransform.BytesToUInt8(BytesData, StaticLength + 8);
                                    cstate = DataTransform.BytesToUInt16(BytesData, StaticLength + 10);
                                    cstn = DataTransform.BytesToUInt16(BytesData, StaticLength + 12);

                                    if (!Enum.IsDefined(typeof(EventStatusCode), status)) { error = 11; break; }

                                    if (BytesLength > StaticLength + 15)
                                    {
                                        m3 = DataTransform.BytesToUInt16(BytesData, StaticLength + 14);

                                        if (BytesLength > StaticLength + 17)
                                        {
                                            lpno = DataTransform.BytesToUInt16(BytesData, StaticLength + 16);
                                            if (lpno > 0 && lpno * 2 + 18 <= BytesLength - StaticLength)
                                            {
                                                lps = new int[lpno];

                                                for (int i = 0; i < lpno; i++)
                                                {
                                                    lps[i] = DataTransform.BytesToUInt16(BytesData, StaticLength + 18 + i * 2);
                                                }
                                            }
                                        }
                                    }

                                    data = new OrderEventAciData()
                                    {
                                        OrderIndex = index,
                                        StructureID = trp,
                                        EventStatus = status,
                                        MagicCode1 = m1,
                                        MagicCode2 = m2,
                                        MagicCode3 = m3,
                                        CarrierID = cid,
                                        CarrierStation = cstn,
                                        CarrierStatus = cstate,
                                        ParamNumber = lpno,
                                        ParamValues = lps
                                    };
                                }
                                break;
                            case MessageType.OrderStatus://订单状态
                                {
                                    int mix = -1, tmpc = 0;

                                    OrderItemCode itemc = OrderItemCode.None;
                                    OrderErrorCode errc = OrderErrorCode.None;
                                    if (BytesLength < StaticLength + 4) { error = 5; break; }

                                    mix = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    tmpc = DataTransform.BytesToUInt16(BytesData, StaticLength + 2);

                                    if (mix > 0x7FFF) { error = 21; break; }

                                    if (tmpc < 0x10)
                                    {
                                        int oix = -1, strp = 0, ctrp = 0, crow = 0, pri = -1, trip1 = -1, trip2 = -1, cid = 0, plcs = 0, pstn = 0, dstn = 0;
                                        uint stime = 0;

                                        OrderListCode olist = OrderListCode.None;
                                        OrderStateCode ostate = OrderStateCode.None;
                                        OrderStatusCode ostatus = OrderStatusCode.None;
                                        OrderTriggerCode trig = OrderTriggerCode.None;
                                        CarrierMainStatusCode mainstat = CarrierMainStatusCode.None;
                                        CarrierMoveStateCode movestat = CarrierMoveStateCode.None;

                                        if (BytesLength < StaticLength + 32) { error = 6; break; }

                                        itemc = (OrderItemCode)tmpc;
                                        oix = DataTransform.BytesToUInt16(BytesData, StaticLength + 4);//看协议数据结构，如果当前这个位于第5个字节，那就是用6加4，6是固定，4的话拿当前字节点减1
                                        stime = DataTransform.BytesToUInt32(BytesData, StaticLength + 8);
                                        strp = DataTransform.BytesToUInt8(BytesData, StaticLength + 12);
                                        ctrp = DataTransform.BytesToUInt8(BytesData, StaticLength + 13);
                                        crow = DataTransform.BytesToUInt8(BytesData, StaticLength + 14);
                                        olist = (OrderListCode)BytesData[StaticLength + 15];
                                        ostate = (OrderStateCode)BytesData[StaticLength + 16];
                                        ostatus = (OrderStatusCode)BytesData[StaticLength + 17];
                                        pri = DataTransform.BytesToUInt8(BytesData, StaticLength + 18);
                                        trig = (OrderTriggerCode)BytesData[StaticLength + 19];
                                        trip1 = DataTransform.BytesToUInt8(BytesData, StaticLength + 20);
                                        trip2 = DataTransform.BytesToUInt8(BytesData, StaticLength + 21);
                                        cid = DataTransform.BytesToUInt8(BytesData, StaticLength + 22);
                                        mainstat = (CarrierMainStatusCode)DataTransform.BytesToUInt8(BytesData, StaticLength + 23);
                                        plcs = DataTransform.BytesToUInt16(BytesData, StaticLength + 24);
                                        movestat = (CarrierMoveStateCode)DataTransform.BytesToUInt16(BytesData, StaticLength + 26);
                                        pstn = DataTransform.BytesToUInt16(BytesData, StaticLength + 28);
                                        dstn = DataTransform.BytesToUInt16(BytesData, StaticLength + 30);

                                        if (!Enum.IsDefined(typeof(OrderItemCode), itemc)) { error = 12; break; }
                                        if (!Enum.IsDefined(typeof(OrderListCode), olist)) { error = 13; break; }
                                        if (!Enum.IsDefined(typeof(OrderStateCode), ostate)) { error = 14; break; }
                                        if (!Enum.IsDefined(typeof(OrderStatusCode), ostatus)) { error = 15; break; }
                                        if (!Enum.IsDefined(typeof(OrderTriggerCode), trig)) { error = 16; break; }
                                        if (!Enum.IsDefined(typeof(CarrierMainStatusCode), mainstat)) { error = 17; break; }
                                        if (!Enum.IsDefined(typeof(CarrierMoveStateCode), movestat)) { error = 18; break; }

                                        data = new OrderStatusAciData()
                                        {
                                            MagicIndex = mix,               //消息索引
                                            ItemCode = itemc,               //订单选择功能码             
                                            ErrorCode = errc,               //错误代码
                                            OrderIndex = oix,               //订单队列中的索引
                                            OrderStartTime = DateTime.Parse("1970/01/01").AddSeconds(stime),//命令开始时间
                                            InitialStructure = strp,        //运输结构，用于启用当前订单
                                            CurrentStructure = ctrp,        //订单当前传输结构
                                            CurrentRow = crow,              //订单当前传输结构行
                                            OrderList = olist,              //订单列表监视器
                                            OrderState = ostate,            //订单状态
                                            OrderStatus = ostatus,          //此订单执行状态
                                            OrderPriority = pri,            //订单优先级
                                            OrderTrigger = trig,            //命令触发器
                                            TriggerParam1 = trip1,          //订单触发参数1
                                            TriggerParam2 = trip2,          //订单触发参数2
                                            CarrierID = cid,                //AGVID
                                            MainStatus = mainstat,          //载体主要状态
                                            PlcStatus = plcs,               //PLC状态
                                            MoveState = movestat,           //移动状态
                                            PreviousStation = pstn,         //分配当前站点
                                            FinalDestination = dstn,        //目的地站点
                                        };
                                    }
                                    else
                                    {
                                        errc = (OrderErrorCode)tmpc;

                                        if (!Enum.IsDefined(typeof(OrderErrorCode), errc)) { error = 19; break; }

                                        data = new OrderStatusAciData()
                                        {
                                            MagicIndex = mix,
                                            ItemCode = itemc,
                                            ErrorCode = errc
                                        };
                                    }
                                }
                                break;
                            case MessageType.LocalParamContents://本地参数
                                {
                                    int index;
                                    int[] pnos = new int[5], pvals = new int[5];

                                    if (BytesLength < StaticLength + 18)
                                    {
                                        error = 7;
                                        break;
                                    }

                                    index = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    for (int i = 0; i < 5; i++)
                                    {
                                        pnos[i] = DataTransform.BytesToUInt8(BytesData, StaticLength + 3 + i);
                                        pvals[i] = DataTransform.BytesToUInt16(BytesData, StaticLength + 8 + i * 2);
                                    }

                                    data = new LocalParamContentsAciData()
                                    {
                                        OrderIndex = index,   //索引
                                        ParamIndexes = pnos,  //参数编号
                                        ParamValues = pvals   //局部参数的值 
                                    };
                                }
                                break;
                            case MessageType.LocalParamRequest:
                                {
                                    int index, pno;

                                    if (BytesLength < StaticLength + 3)
                                    {
                                        error = 7;
                                        break;
                                    }

                                    index = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    pno = DataTransform.BytesToUInt8(BytesData, StaticLength + 2);

                                    data = new LocalParamRequestAciData()
                                    {
                                        OrderIndex = index,     //订单队列中的索引
                                        ParamIndex = pno        //参数编号
                                    };
                                }
                                break;
                            case MessageType.GlobalParamStatus://全局参数状态
                                {
                                    int mix, pnum, pix;
                                    GlobalParamStatusCode code = GlobalParamStatusCode.None;
                                    int[] pvals;

                                    if (BytesLength < StaticLength + 6) { error = 8; break; }

                                    mix = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                    code = (GlobalParamStatusCode)DataTransform.BytesToUInt8(BytesData, StaticLength + 2);
                                    pnum = DataTransform.BytesToUInt8(BytesData, StaticLength + 3);//访问的参数数量
                                    pix = DataTransform.BytesToUInt16(BytesData, StaticLength + 4);//消息位置P0中的全局参数号

                                    if (mix > 0x7FFF) { error = 21; break; }
                                    if (pnum < 1 || pnum > 16) { error = 22; break; }
                                    if (pix > 10000) { error = 22; break; }
                                    if (!Enum.IsDefined(typeof(GlobalParamStatusCode), code)) { error = 20; break; }

                                    if (code == GlobalParamStatusCode.ReadAck)
                                    {
                                        if (BytesLength < StaticLength + 6 + pnum * 2) { error = 9; break; }

                                        pvals = new int[pnum];

                                        for (int i = 0; i < pnum; i++)
                                        {
                                            pvals[i] = DataTransform.BytesToUInt16(BytesData, StaticLength + 6 + i * 2);
                                        }
                                    }

                                    else
                                    {
                                        pvals = new int[] { };
                                    }

                                    data = new GlobalParamStatusAciData()
                                    {
                                        MagicIndex = mix,           //索引
                                        Status = code,              //功能码
                                        ParamNumber = pnum,
                                        ParamIndex = pix,
                                        ParamValues = pvals
                                    };
                                    GlobalParamValues = pvals;
                                    ParamIndex = pix;
                                }
                                break;
                            case MessageType.HostToAgvOmPlc:

                                break;
                            case MessageType.AgvOmPlcToHost:
                                //车id,   magic, 偏移量,多个字节是从0开始，读取总数
                                int carId, magic;
                                OmPlcCommandCode codes = OmPlcCommandCode.None;//功能码
                                int[] pval;//数据
                                if (BytesLength < StaticLength + 8) { error = 88; break; }

                                carId = DataTransform.BytesToUInt16(BytesData, StaticLength);
                                magic = DataTransform.BytesToUInt16(BytesData, StaticLength + 2);
                                codes = (OmPlcCommandCode)DataTransform.BytesToUInt8(BytesData, StaticLength + 4);


                                if (carId < 1 || carId > 100) { error = 22; break; }
                                if (magic > 0xFFFF) { error = 21; break; }
                                if (!Enum.IsDefined(typeof(OmPlcCommandCode), codes)) { error = 20; break; }

                                int pnum1 = (BytesLength - StaticLength - 10) / 2;  //获取要读取的字节数

                                if (codes == OmPlcCommandCode.ReadMultiByte)
                                {
                                    pval = new int[pnum1];

                                    for (int i = 0; i < pnum1; i++)
                                    {
                                        pval[i] = DataTransform.BytesToReverseUInt16(BytesData, StaticLength + 10 + i * 2);//6
                                    }
                                }
                                else if (codes == OmPlcCommandCode.WriteMultiByte)
                                {
                                    pval = new int[pnum1];

                                    for (int i = 0; i < pnum1; i++)
                                    {
                                        pval[i] = DataTransform.BytesToReverseUInt16(BytesData, StaticLength + 10 + i * 2);
                                    }
                                }
                                else
                                {
                                    pval = new int[] { };

                                }

                                data = new OmPlcStatusAciData()
                                {
                                    MagicIndex = magic,
                                    ParamCarID = carId,
                                    Status = codes,
                                    ParamValues = pval
                                };

                                break;
                            default:
                                {
                                    error = 2;
                                }
                                break;
                        }
                    }
                }
                else if (func == FunctionCode.HeartBeatPoll)
                {
                    data = new GeneralAciData()
                    {
                        DataFunction = FunctionCode.HeartBeatPoll,
                        DataType = MessageType.None
                    };
                }
                else
                {
                    error = 1;
                }
            }

            if (data == null)
            {
                data = new GeneralAciData()
                {
                    DataFunction = func,
                    DataType = mType,
                    CompileErrorCode = error
                };
            }

            SetApplication(data);
        }
        //取解析数据并且要使用
        protected override void OnDecompileData()
        {
            if (!(ApplicationData is GeneralAciData)) return;
            GeneralAciData gdata = ApplicationData as GeneralAciData;

            byte[] bytes = null;
            gdata.DecompileErrorCode = 0;

            switch (gdata.DataFunction)
            {
                case FunctionCode.Normal: //正常
                    {
                        byte[] msg = null;

                        switch (gdata.DataType)
                        {
                            case MessageType.OrderInitiate:
                                {
                                    OrderInitiateAciData data = gdata as OrderInitiateAciData;

                                    if (data.ParamValues == null) data.ParamValues = new int[] { };
                                    if (data.StructureID < 1 || data.StructureID > 255) { data.DecompileErrorCode = 3; break; }
                                    if (data.OrderPriority > 99) { data.DecompileErrorCode = 4; break; }

                                    int pnum = data.ParamValues.Length;

                                    msg = new byte[6 + pnum * 2];
                                    DataTransform.UInt8ToBytes(data.StructureID, msg, 0);                  //传输结构 
                                    DataTransform.UInt8ToBytes(data.OrderPriority + 0x80, msg, 1);         //优先级
                                    DataTransform.UInt16ToBytes(0x0001, msg, 2);                           //功能码
                                    DataTransform.UInt16ToBytes(data.OrderKey, msg, 4);                    //Key

                                    for (int i = 0; i < pnum; i++)
                                    {
                                        DataTransform.UInt16ToBytes(data.ParamValues[i], msg, 6 + i * 2);  //参数值
                                    }
                                }
                                break;
                            case MessageType.OrderRequest:
                                {
                                    OrderRequestAciData data = gdata as OrderRequestAciData;

                                    if (data.MagicIndex > 0x7FFF) { data.DecompileErrorCode = 6; break; }
                                    if (data.ItemOffset > 0xFFFF) { data.DecompileErrorCode = 7; break; }
                                    if (data.ItemNumber < 1 || data.ItemNumber > 0xFFFF) { data.DecompileErrorCode = 8; break; }

                                    switch (data.ItemCode) //功能码
                                    {
                                        case OrderItemCode.NumericalInterval:
                                        case OrderItemCode.UsedIndex:
                                            {
                                                msg = new byte[12]; //对应协议A类型数组

                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);           //索引
                                                DataTransform.UInt8ToBytes((int)data.ItemCode, msg, 3);         //订单状态代码
                                                DataTransform.UInt16ToBytes(data.ItemOffset, msg, 4);           //项目偏称
                                                DataTransform.UInt16ToBytes(data.ItemNumber, msg, 6);           //从项目组请求的项目数
                                                DataTransform.UInt32ToBytes(data.ParamFlag, msg, 8);            //参数标志
                                            }
                                            break;
                                        case OrderItemCode.ExternalTrigger:
                                            {
                                                msg = new byte[18];//协议B类型

                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);          //索引
                                                DataTransform.UInt8ToBytes(data.MagicIndex, msg, 3);           //订单状态代码
                                                DataTransform.UInt16ToBytes(data.ItemOffset, msg, 4);          //项目偏称
                                                DataTransform.UInt16ToBytes(data.ItemNumber, msg, 6);          //从项目组请求的项目数
                                                DataTransform.UInt32ToBytes(data.ParamFlag, msg, 8);           //参数标志
                                                DataTransform.UInt16ToBytes((int)data.UnitType, msg, 12);      //IO单元类型
                                                DataTransform.UInt16ToBytes(data.LogicalLine, msg, 14);        //逻辑号
                                                DataTransform.UInt16ToBytes(data.LogicalUnit, msg, 16);        //单元ID号
                                            }
                                            break;
                                        case OrderItemCode.InternalTrigger:
                                            {
                                                msg = new byte[16]; //协议C类型

                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);
                                                DataTransform.UInt8ToBytes(data.MagicIndex, msg, 3);
                                                DataTransform.UInt16ToBytes(data.ItemOffset, msg, 4);
                                                DataTransform.UInt16ToBytes(data.ItemNumber, msg, 6);
                                                DataTransform.UInt32ToBytes(data.ParamFlag, msg, 8);
                                                DataTransform.UInt16ToBytes((int)data.TriggerModule, msg, 12);
                                            }
                                            break;
                                        case OrderItemCode.CarrierNumber:
                                        case OrderItemCode.PriorityOrder:
                                        case OrderItemCode.OrderState:
                                            {
                                                msg = new byte[14]; //协议D类型

                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);
                                                DataTransform.UInt8ToBytes(data.MagicIndex, msg, 3);
                                                DataTransform.UInt16ToBytes(data.ItemOffset, msg, 4);
                                                DataTransform.UInt16ToBytes(data.ItemNumber, msg, 6);
                                                DataTransform.UInt32ToBytes(data.ParamFlag, msg, 8);
                                                DataTransform.UInt16ToBytes((int)data.ItemParam, msg, 12);
                                            }
                                            break;
                                        default:
                                            {
                                                data.DecompileErrorCode = 5;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case MessageType.OrderDelete:
                                {
                                    OrderDeleteAciData data = gdata as OrderDeleteAciData;

                                    if (data.CarrierID <= 0)
                                    {
                                        if (data.OrderIndex > 0xFFFF) { data.DecompileErrorCode = 9; break; }

                                        msg = new byte[2];

                                        DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                    }
                                    else
                                    {
                                        if (data.CarrierID < 1 || data.CarrierID > 255) { data.DecompileErrorCode = 10; break; }

                                        msg = new byte[3];

                                        DataTransform.UInt16ToBytes(0, msg, 0);
                                        DataTransform.UInt8ToBytes(data.CarrierID, msg, 2);
                                    }
                                }
                                break;
                            case MessageType.LocalParamCommand:
                                {
                                    LocalParamCommandAciData data = gdata as LocalParamCommandAciData;

                                    if (data.OrderIndex > 0xFFFF) { data.DecompileErrorCode = 9; break; }

                                    switch (data.Command)
                                    {
                                        case LocalParamCommandCode.InsertSpontaneous:
                                        case LocalParamCommandCode.InsertRequested:
                                            {
                                                if (data.ParamIndex > 31) { data.DecompileErrorCode = 12; break; }
                                                if (data.ParamValues == null) { data.DecompileErrorCode = 13; break; }

                                                int pnum = data.ParamValues.Length;
                                                msg = new byte[4 + pnum * 2];

                                                DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.ParamIndex, msg, 3);

                                                for (int i = 0; i < pnum; i++)
                                                {
                                                    DataTransform.UInt16ToBytes(data.ParamValues[i], msg, 4 + i * 2);
                                                }
                                            }
                                            break;
                                        case LocalParamCommandCode.Delete:
                                            {
                                                if (data.ParamIndex > 31) { data.DecompileErrorCode = 12; break; }
                                                msg = new byte[4];

                                                DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.ParamIndex, msg, 3);
                                            }
                                            break;
                                        case LocalParamCommandCode.Read:
                                            {
                                                if (data.ParamValues == null) { data.DecompileErrorCode = 13; break; }
                                                int pnum = data.ParamValues.Length < 5 ? data.ParamValues.Length : 5;

                                                msg = new byte[8];

                                                DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                for (int i = 0; i < pnum; i++)
                                                {
                                                    DataTransform.UInt8ToBytes(data.ParamValues[i], msg, 3 + i);
                                                }
                                            }
                                            break;
                                        case LocalParamCommandCode.ChangePriority:
                                            {
                                                if (data.OrderPriority > 99) { data.DecompileErrorCode = 4; break; }

                                                msg = new byte[4];

                                                DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.OrderPriority, msg, 3);
                                            }
                                            break;
                                        case LocalParamCommandCode.ConnectVehicle:
                                            {
                                                if (data.CarrierID < 1 || data.CarrierID > 255) { data.DecompileErrorCode = 10; break; }

                                                msg = new byte[4];

                                                DataTransform.UInt16ToBytes(data.OrderIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.OrderPriority, msg, 3);
                                            }
                                            break;
                                        default:
                                            {
                                                data.DecompileErrorCode = 11;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case MessageType.GlobalParamCommand:
                                {
                                    GlobalParamCommandAciData data = gdata as GlobalParamCommandAciData;

                                    if (data.ParamValues == null) data.ParamValues = new int[] { };
                                    if (data.MagicIndex > 0x7FFF) { data.DecompileErrorCode = 6; break; }
                                    if (data.ParamNumber < 1 || data.ParamNumber > 16) { data.DecompileErrorCode = 14; break; }
                                    if (data.ParamIndex > 100000) { data.DecompileErrorCode = 15; break; }

                                    switch (data.Command)
                                    {
                                        case GlobalParamCommandCode.Read:
                                            {
                                                msg = new byte[6];
                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.ParamNumber, msg, 3);
                                                DataTransform.UInt16ToBytes(data.ParamIndex, msg, 4);
                                            }
                                            break;
                                        case GlobalParamCommandCode.Write:
                                            {
                                                int pnum = data.ParamValues.Length;
                                                msg = new byte[6 + pnum * 2];
                                                DataTransform.UInt16ToBytes(data.MagicIndex, msg, 0);
                                                DataTransform.UInt8ToBytes((int)data.Command, msg, 2);
                                                DataTransform.UInt8ToBytes(data.ParamNumber, msg, 3);
                                                DataTransform.UInt16ToBytes(data.ParamIndex, msg, 4);
                                                for (int i = 0; i < pnum; i++)
                                                {
                                                    DataTransform.UInt16ToBytes(data.ParamValues[i], msg, 6 + i * 2);
                                                }
                                            }
                                            break;
                                        default:
                                            {
                                                data.DecompileErrorCode = 16;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case MessageType.HostToAgvOmPlc:
                                OmPlcCommandAciData datas = gdata as OmPlcCommandAciData;
                                if (datas.ParamValues == null) datas.ParamValues = new int[] { };
                                if (datas.MagicIndex > 0xFFFF) { datas.DecompileErrorCode = 6; break; } //消息魔法不能超过65535
                                if (datas.ParamCarId < 1 || datas.ParamCarId > 100) { datas.DecompileErrorCode = 14; break; }//agcid
                                if (datas.ParamIndex < 0 || datas.ParamIndex > 239) { datas.DecompileErrorCode = 15; break; ; } //plc偏移量
                                if (datas.ParamNumber < 1 || datas.ParamNumber > 240) { datas.DecompileErrorCode = 16; break; } //读取的总数不能超过240

                                switch (datas.Command)
                                {
                                    case OmPlcCommandCode.ReadMultiByte:
                                        {
                                            #region 很慢的协议
                                            //msg = new byte[10];
                                            //DataTransform.UInt8ToBytes(0, msg, 0);
                                            //DataTransform.UInt8ToBytes(datas.ParamCarId, msg, 1);
                                            //DataTransform.UInt8ToBytes(datas.MagicIndex, msg, 2);
                                            //DataTransform.UInt8ToBytes(0, msg, 3); //新增加
                                            //DataTransform.UInt8ToBytes((int)datas.Command, msg, 4);
                                            //DataTransform.UInt8ToBytes(0, msg, 5);
                                            //DataTransform.UInt8ToBytes(0, msg, 6);
                                            //DataTransform.UInt8ToBytes(datas.ParamIndex, msg, 7);
                                            //DataTransform.UInt8ToBytes(datas.ParamNumber, msg, 8);
                                            //DataTransform.UInt8ToBytes(0, msg, 9);
                                            #endregion
                                            msg = new byte[10];
                                            DataTransform.UInt16ToBytes(datas.ParamCarId, msg, 0);
                                            DataTransform.UInt16ToBytes(datas.MagicIndex, msg, 2);
                                            DataTransform.UInt8ToBytes((int)datas.Command, msg, 4);
                                            DataTransform.UInt8ToBytes(0, msg, 5);
                                            DataTransform.UInt16ToBytes(datas.ParamIndex, msg, 6);
                                            DataTransform.UInt8ToBytes(datas.ParamNumber, msg, 8);
                                            DataTransform.UInt8ToBytes(0, msg, 9);
                                        }
                                        break;
                                    case OmPlcCommandCode.WriteMultiByte:
                                        {
                                            int pnum = datas.ParamValues.Length;
                                            msg = new byte[10 + pnum * 2];
                                            DataTransform.UInt16ToBytes(datas.ParamCarId, msg, 0);
                                            DataTransform.UInt16ToBytes(datas.MagicIndex, msg, 2);
                                            DataTransform.UInt8ToBytes((int)datas.Command, msg, 4);
                                            DataTransform.UInt8ToBytes(0, msg, 5);
                                            DataTransform.UInt16ToBytes(datas.ParamIndex, msg, 6);
                                            DataTransform.UInt8ToBytes(datas.ParamNumber, msg, 8);
                                            DataTransform.UInt8ToBytes(0, msg, 9);
                                            for (int i = 0; i < pnum; i++)
                                            {
                                                DataTransform.UInt16ToReverseBytes(datas.ParamValues[i], msg, 10 + i * 2);
                                            }
                                        }
                                        break;
                                }

                                break;
                            default:
                                {
                                    gdata.DecompileErrorCode = 2;
                                }
                                break;
                        }

                        if (msg != null)
                        {
                            int rSize = msg.Length;

                            int mSize = msg.Length;
                            if (mSize % 2 != 0) mSize += 1;
                            bytes = new byte[6 + mSize];

                            DataTransform.UInt16ToBytes((int)gdata.DataFunction, bytes, 0);
                            DataTransform.UInt16ToBytes((int)gdata.DataType, bytes, 2);
                            DataTransform.UInt16ToBytes(rSize, bytes, 4);

                            Buffer.BlockCopy(msg, 0, bytes, 6, rSize);
                        }
                    }
                    break;
                case FunctionCode.HeartBeatACK:
                    {
                        bytes = new byte[2];
                        DataTransform.UInt16ToBytes((int)gdata.DataFunction, bytes, 0);
                    }
                    break;
                default:
                    gdata.DecompileErrorCode = 1;
                    break;
            }

            SetBytes(bytes);
        }
    }
}
