using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PDS.Application.Controcts;
using PDS.Domain.Entitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace PDS.Application.Worker
{
    /// <summary>
    /// 负责告诉HB系统，站点上线，需要agv在指定点排队。
    /// </summary>
    public class OpStationOnlineNoticeToHBWorker : RepeatBackgroundWorkerBase, ITransientDependency  //默认是自动注册成单例的，加了ITransientDependency则注册成Transient；
    {
        const string HeartBeatUrl = "/api/PDS/HeartBeat";
        private readonly IRepository<OperationStation, string> operationStationsRepos;
        private readonly ISettingProvider settingProvider;
        private readonly ILogger<PDSApplicationModule> logger;

        public static DateTime LastSuccessNoticeTime;
        public OpStationOnlineNoticeToHBWorker(
            IRepository<OperationStation,string> operationStationsRepos,
            ISettingProvider settingProvider,
            ILogger<PDSApplicationModule> logger) : base(10)  //10s执行一次。
        {
            this.operationStationsRepos = operationStationsRepos;
            this.settingProvider = settingProvider;
            this.logger = logger;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            var HBUrl = await this.settingProvider.GetOrNullAsync(PDSSettingDefinitionProvider.HBUrl);
            if (HBUrl.IsNullOrWhiteSpace()) this.logger.LogError("无法获取HBUrl");

            var OpStations = await this.operationStationsRepos.GetListAsync();
            List<StationParam> OnlineNodeName = new List<StationParam>();
            foreach (var Station in OpStations)
            {
                if (Station.State == OperationStationState.ONLINE)
                {
                    OnlineNodeName.Add(new StationParam() { MaxAgv = Station.MaxAgvCount, Station = Station.MapNodeName });
                }
            }


            var requestCode = Guid.NewGuid().ToString();
            var resulct = HttpClientHelper.Post<HeartBeatResponse>(HBUrl + HeartBeatUrl, new HeartBeatToHBPara()
            {
                OnLineStations = OnlineNodeName,
                RequestCode = requestCode
            });

            if(resulct != null && resulct.requestCode == requestCode)
            {
                LastSuccessNoticeTime = DateTime.Now;
            }

        }
    }



   

    public class HeartBeatToHBPara
    {
        public string RequestCode { get; set; }
        public List<StationParam> OnLineStations { get; set; }
    }

    public class HeartBeatResponse
    {
        public string requestCode { get; set; }
        public List<StationResponse> response { get; set; }
    }

     public class StationResponse
    {
        /// <summary>
        /// 反馈操作状态， 0表示成功，其他值表示失败
        /// </summary>
        public int responseCode = -1;

        public string describe = "";

        public override string ToString()
        {
            return "reponseCode = " + responseCode + ":describe=" + describe;
        }

    }


    public class StationParam
    {
        /// <summary>
        /// 操作站点
        /// </summary>
        public string Station;

        /// <summary>
        /// 站点的agv台数
        /// </summary>
        public int MaxAgv = -1;
    }
}
