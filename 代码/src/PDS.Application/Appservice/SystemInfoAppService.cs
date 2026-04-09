using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PDS.Application.Contracts.Dtos;
using PDS.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PDS.Domain.Entitys;
using Volo.Abp.Domain.Repositories;
using System.Data.SqlClient;
using System.Linq;
using Volo.Abp.Data;
using PDS.Application.Controcts;
using Volo.Abp.DependencyInjection;
using Volo.Abp;
using PDS.Application.Interface;
using Newtonsoft.Json;


namespace PDS
{
    //[RemoteService(false)]
    [Route("PDS/api/SystemInfo")]
    public class SystemInfoAppService : PDSAppService
    {


        public SystemInfoAppService(

            )
        {

        }



        [HttpPost("HBCommInfo")]
        public Task<HBCommInfoDto> HBCommInfo()
        {
            var time = Application.Worker.OpStationOnlineNoticeToHBWorker.LastSuccessNoticeTime;
            if ((DateTime.Now - time).TotalSeconds > 30)
            {
                return Task.FromResult(new HBCommInfoDto() { commStatus = CommStatus.OutLine, LastCommTime = time });
            }
            else
            {
                return Task.FromResult(new HBCommInfoDto() { commStatus = CommStatus.OnLine, LastCommTime = time });
            }
        }
    }
}
