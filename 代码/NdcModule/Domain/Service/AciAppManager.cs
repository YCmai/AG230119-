using AciModule.Domain.Entitys;
using AciModule.Domain.Shared;
using System.Collections.Concurrent;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Uow;

namespace AciModule.Domain.Service
{
    public class AciAppManager : ISingletonDependency
    {
        private readonly AciConnection _aciClient;
        private readonly ConcurrentQueue<AciEvent> _aciEventQueue;
        private readonly ILocalEventBus _eventBus;
        public int AciEventCount { get; set; } = 0;
        public AciConnection AciClient => _aciClient;
        public ConcurrentQueue<AciEvent> AciEventQueue => _aciEventQueue;
        public AciAppManager(ILocalEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _aciEventQueue = new ConcurrentQueue<AciEvent>();
            _aciClient = new AciConnection();

            _aciClient.RequestDataReceived += AciClient_RequestDataReceived; // 通讯事件
            _aciClient.ConnectedChanged += AciClient_ConnectedChanged; // 连接事件
            _aciClient.SetServerLocalIP(31000);
        }
        [UnitOfWork]
        public virtual async void AciClient_RequestDataReceived(object sender, AciDataEventArgs e)
        {
             System.Diagnostics.Debug.WriteLine("事件触发");
              await _eventBus.PublishAsync(eventData: e);
        }
        private void AciClient_ConnectedChanged(object sender, EventArgs e)
        {
            SendGlobalParamRead(InitialHostCallBack, 0, 1);
        }

        //连接成功后给GP写2
        private void InitialHostCallBack(AciCommandData data)
        {
            if (data.Acknowledged)
            {
                if (data.AcknowledgeData is GlobalParamStatusAciData)
                {
                    GlobalParamStatusAciData? ack = data.AcknowledgeData as GlobalParamStatusAciData;
                    if (ack.ParamValues.Length >= 1)
                    {
                        if (ack.ParamValues[0] == 2)
                        {
                            SendGlobalParamWrite(null, 0, 1, new int[] { 2 });

                            Thread.Sleep(3000);

                            SendGlobalParamWrite(null, 0, 1, new int[] { 2 });

                            return;
                        }
                        else
                        {
                            SendGlobalParamWrite(null, 0, 1, new int[] { 2 });
                        }
                    }
                }
            }
            if (AciClient.Connected)
            {
                SendGlobalParamRead(InitialHostCallBack, 0, 1);
            }
        }
        public void AciEventAdd(AciEvent e)
        {
            AciEventQueue.Enqueue(e);

            if (AciEventCount++ > 100)
            {
                AciEvent remove;
                if (AciEventQueue.TryDequeue(out remove))
                {
                    remove.SetOverDate();
                    AciEventCount--;
                }
            }
        }
        //读取GP
        public AciCommandData SendGlobalParamRead(AciCommandCallBack callback, int index, int number)
        {
            return AciClient.SendGlobalParamRead(callback, index, number);
        }
        //写GP
        public AciCommandData SendGlobalParamWrite(AciCommandCallBack callback, int index, int number, int[] vals)
        {
            return AciClient.SendGlobalParamWrite(callback, index, number, vals);
        }
        //回复数据给固定的ts18
        public AciCommandData SendHostAcknowledge(AciCommandCallBack callback, int oix, int hostack, int ack1, int ack2)
        {
            return SendLocalParamInsert(callback, oix, 18, new int[] { hostack, ack1, ack2 });
        }
        public AciCommandData SendLocalParamInsert(AciCommandCallBack callback, int oix, int pix, int[] pvals)
        {
            return AciClient.SendLocalParamInsert(callback, oix, pix, pvals);
        }
        //发送任务
        public AciCommandData SendOrderInitial(AciCommandCallBack callback, int key, int trp, int pri, int[] vals)
        {
            return AciClient.SendOrderInitial(callback, key, trp, pri, vals);
        }


        //取消任务
        public AciCommandData SendOrderDeleteViaOrder(AciCommandCallBack callback, int index)
        {
            return AciClient.SendOrderDeleteViaOrder(callback, index);
        }
    }
}
