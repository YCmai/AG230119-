//using PDS.Application.Controcts;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.DependencyInjection;

//namespace PDS.Application.Worker
//{
//    /// <summary>
//    /// 负责工作站超时检查
//    /// </summary>
//    public class TestWorker : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
//    {
//        const string OnlineUrl = "https://localhost:44340/PDS/api/OperationStation/OpStationOnLineSignal";
//        private readonly OperationStationAppService operationStationAppService;

//        public TestWorker(OperationStationAppService operationStationAppService) : base(1)  //10s执行一次。
//        {
//            this.operationStationAppService = operationStationAppService;
//        }


//        //定时触发上线信号，模拟上线。
//        public override async Task Execute(IJobExecutionContext context)
//        {
//            await HttpClientHelper.Post<object>(OnlineUrl, new OpStationOnLineInput()  //这里假设Op001是上线的。
//            {
//                FrameId = Guid.NewGuid().ToString(),
//                OperationStationId = "Op001"
//            }
//            );

//            await HttpClientHelper.Post<object>(OnlineUrl, new OpStationOnLineInput()  //这里假设Op001是上线的。
//            {
//                FrameId = Guid.NewGuid().ToString(),
//                OperationStationId = "Op002"
//            }
//);

//            await HttpClientHelper.Post<object>(OnlineUrl, new OpStationOnLineInput()  //这里假设Op001是上线的。
//            {
//                FrameId = Guid.NewGuid().ToString(),
//                OperationStationId = "Op003"
//            }
//);

//            await HttpClientHelper.Post<object>(OnlineUrl, new OpStationOnLineInput()  //这里假设Op001是上线的。
//            {
//                FrameId = Guid.NewGuid().ToString(),
//                OperationStationId = "Op004"
//            }
//);
//        }
//    }


//    public class OpStationOnLineInput
//    {
//        public string FrameId { get; set; }
//        public string OperationStationId { get; set; }
//    }


//}
