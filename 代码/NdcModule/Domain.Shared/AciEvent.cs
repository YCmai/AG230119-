
namespace AciModule.Domain.Shared
{
    public class AciEvent
    {
        private static int IDSeed = 0;

        private int _ID = IDSeed++;
        private DateTime _Time = DateTime.Now;
        private bool _OverDate = false;

        public int ID { get { return _ID; } }
        public int Index { get; set; }
        public AciHostEventTypeEnum Type { get; set; }
        public int Parameter1 { get; set; }
        public int Parameter2 { get; set; }
        public DateTime Time { get { return _Time; } }
        public bool OverDate { get { return _OverDate; } }

        public void SetOverDate()
        {
            _OverDate = true;
        }

        public override string ToString()
        {
            string output;
            switch (Type)
            {
                case AciHostEventTypeEnum.WarmStart:
                    output = string.Format("{0}：系统已热重启！", Time);
                    break;
                case AciHostEventTypeEnum.ColdStart:
                    output = string.Format("{0}：系统已冷启动！", Time);
                    break;
                case AciHostEventTypeEnum.HostSync:
                    output = string.Format("{0}：订单任务({3:0000})已执行到第{1:00}步！等待Host指令！", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.OrderReceived:
                    output = string.Format("{0}：订单任务({3:0000})已接受！从{1:0000}配送至{2:0000}。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarrierConnected:
                    output = string.Format("{0}：发现小车{1:00}要洗车,已分配至订单任务({3:0000})！", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.OrderTransform:
                    output = string.Format("{0}：小车{1:00}通过再匹配，分配至订单任务({3:0000})！", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.OrderCancel:
                    output = string.Format("{0}：订单任务({3:0000})已取消！已执行到第{1:00}步。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.OrderComplete:
                    output = string.Format("{0}：订单任务({3:0000})已完成！从{1:0000}配送至{2:0000}。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.ValidKey:
                    output = string.Format("{0}：订单任务({3:0000})失败！无效的Key。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.FetchError:
                    output = string.Format("{0}：订单任务({3:0000})失败！无效的上料点({1:0000})。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.DeliverError:
                    output = string.Format("{0}：订单任务({3:0000})失败！无效的下料点({1:0000})。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarWashRequest:
                    output = string.Format("{0}：小车{1:00}正在初始化！等待下料站点指令。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarWashFailed:
                    output = string.Format("{0}：小车{1:00}初始化失败！下料站点({2:0000})错误。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarWashComplete:
                    output = string.Format("{0}：小车{1:00}初始化完成或取消！", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarChargeStart:
                    output = string.Format("{0}：小车{1:00}开始充电。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarChargeEnd:
                    output = string.Format("{0}：小车{1:00}完成充电。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarChargeError:
                    output = string.Format("{0}：小车{1:00}充电时发生错误！错误代码：{2:000}(0x{2:X2})。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.CarrierContinue:
                    output = string.Format("{0}：小车{1:00}初始化完成！继续任务({2:00000})。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.Location:
                    output = string.Format("{0}：小车x：{1:00},y{2:00}({3:00000})。", Time, Parameter1, Parameter2, Index);
                    break;
                //新增加
                case AciHostEventTypeEnum.OrderStart:
                    output = string.Format("{0}：订单开始:未知参数:{1},任务ID:{2},订单索引:{3}。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.ParameterCheck:
                    output = string.Format("{0}：参数确认:取货点:{1},卸货点:{2},订单索引:{3}。", Time, Parameter1, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.MoveToLoad:
                    output = string.Format("{0}：接受任务的车辆:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.LoadHostSyncronisation:
                    output = string.Format("{0}：到达取货站点入口:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.Loading:
                    output = string.Format("{0}：AGV取货完成:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.UnloadHostSyncronisation:
                    output = string.Format("{0}：到达卸货站点入口:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.Unloading:
                    output = string.Format("{0}：AGV卸货完成:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.OrderFinish:
                    output = string.Format("{0}：订单完成:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.Redirect:
                    output = string.Format("{0}：订单重定向:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.Cancel:
                    output = string.Format("{0}：订单结束:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.InvalidDeliverStation:
                    output = string.Format("{0}：无效的卸货站点:任务索引:{1},订单索引:{2}。", Time, Parameter2, Index);
                    break;
                case AciHostEventTypeEnum.ConnectionCarrier:
                    output = string.Format("{0}连接载体", Time);
                    break;
                default:
                    output = string.Format("{0}：未知事件。", Time);
                    break;
            }
            return output;
        }
    }
}
