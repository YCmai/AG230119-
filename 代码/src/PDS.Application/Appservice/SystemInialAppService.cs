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
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using MapLib;
using Volo.Abp.Domain.Repositories;
using PDS.Domain.Entitys;

namespace PDS
{
    public class SystemInialAppService : PDSAppService  
    {
        private readonly ILogger<SystemInialAppService> logger;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;

        public SystemInialAppService(IRepository<DeliverOutlet,string> deliverOutletsRepos) 
        {
            this.deliverOutletsRepos = deliverOutletsRepos;
        }
        [HttpPost]
        [Route("api/SystemInial/ClearAndReInialOpStationFromMap")]
        public async Task<IActionResult> ClearAndReInialOpStationFromMap(IFormFile formFile)  //List<IFormFile>则表示一次上传多个文件。
        {
            var filePath = @"D:\" + formFile.FileName;  //把文件存在d盘。按原来的文件名称。

            if (formFile.Length > 0)
            {
                using (var stream = new FileStream(filePath,FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                    MapInfo mapInfo = new MapInfo(new Map(1, "a"));
                    mapInfo.Inial(filePath);


                }
            }

            //todo：获取地图的操作站点信息。对比差异。地图有，但数据库没有的，添加。  地图没有，但数据库有的删除。
            //从地图中获取操作点信息；
            
            return new JsonResult(new { Result = "Fault" , Message = "输出有误。" });
        }


        //todo,上面是实现操作点，类似的，可以实现从地图读取分拣车投递点；笼车储位（CageCarStorage）、
    }
}
