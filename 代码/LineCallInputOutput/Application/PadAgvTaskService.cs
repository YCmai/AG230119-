using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.LineCallInputModule.Domain;
using Volo.Abp.Application.Services;
using WMS.MaterialModule.Domain;
using WMS.StorageModule.Domain;
using Volo.Abp.EventBus.Local;
using HBTaskModule.Application;
using Volo.Abp.Guids;
using Volo.Abp.Application.Dtos;
using System.Linq;
using AciModule.Domain.Entitys;
using AciModule.Domain.Shared;
using System.Xml.Linq;
using System.Runtime.Serialization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;
using Quartz.Logging;

namespace WMS.LineCallInputModule.Application
{
    [DisableAuditing]
    [Route("/api/task/padagvtask")]
    public class PadAgvTaskService : ApplicationService
    {

        private readonly ILogger<PadAgvTaskService> _logger;
        private readonly IRepository<NdcTask_Moves, Guid> _ndcTaskRepos;
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly IGuidGenerator _guidGenerator;
       

        public PadAgvTaskService(ILogger<PadAgvTaskService> logger,

            IRepository<NdcTask_Moves, Guid> ndcTaskRepos,
            IGuidGenerator guidGenerator,
            IRepository<Storage, string> storageRepos
        
          
            )
        {

            _logger = logger;
            _storageRepos = storageRepos;
            _ndcTaskRepos = ndcTaskRepos;
          
            _guidGenerator = guidGenerator;
        }


        public string GetTimeStamp()
        {
            return Guid.NewGuid().ToString();
        }

        [HttpPost("bindOperation")]
        public async Task BindOperation(SavebindOperation input)
        {

            if (input != null && input.SelectedRowKeys != null && input.SelectedRowKeys.Count > 0)
            {
                foreach (var item in input.SelectedRowKeys)
                {
                    var storModel = await _storageRepos.FindAsync(x => x.Id == item);

                    if (storModel != null)
                    {
                        switch (input.Value)
                        {
                            //批量启用储位
                            case 1:
                                storModel.AppData1 = "开启";
                                await _storageRepos.UpdateAsync(storModel);
                               
                                break;
                            //批量禁用储位
                            case 2:
                                storModel.AppData1 = "禁用";
                                await _storageRepos.UpdateAsync(storModel);
                              
                                break;
                            //批量清除物料数量
                            case 3:
                                storModel.SetMaterialCount(0);
                                await _storageRepos.UpdateAsync(storModel);
                             
                                break;
                        }
                    }
                }

            }
            else
            {
                throw new BusinessException("无效的输入");
            }

          
        }
    }


    public class SavebindOperation
    {
        public List<string> SelectedRowKeys { get; set; }
        public int Value { get; set; }
    }
}
