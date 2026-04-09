using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PDS.Domain.Entitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using PDS.Application.Interface;
using PDS.Domain.Shared;
using PDS.Application.Controcts;
using PDS.Application.Contracts.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    [Route("PDS/api/CageCar")]
    public class CageCarAppService : PDSAppService
    {
        private readonly IRepository<CageCar, string> cageCarsRepos;
        private readonly IRepository<CageCarStorage, string> cageCarStoragesRepos;
        private readonly IRepository<PackageSort, string> packageSortsRepos;

        public CageCarAppService(
            IRepository<CageCar, string> cageCarsRepos,
            IRepository<CageCarStorage,string> cageCarStoragesRepos, 
            IRepository<PackageSort,string> packageSortsRepos
            )
        {
            this.cageCarsRepos = cageCarsRepos;
            this.cageCarStoragesRepos = cageCarStoragesRepos;
            this.packageSortsRepos = packageSortsRepos;
        }
        [HttpGet("GetStorages")]
        public async Task<List<GetCageCarStorageDto>> GetAllStorages()
        {
            var entities = await cageCarStoragesRepos.GetListAsync();
            return ObjectMapper.Map<List<CageCarStorage>, List<GetCageCarStorageDto>>(entities);
        }

        [HttpGet("Get")]
        public async Task<CageCarCreateInput> GetAsync(string Id)
        {
            var entity = await cageCarsRepos.GetAsync(m => m.Id == Id);
            if (entity == null)
                return null;
            return new CageCarCreateInput { CageCaId = entity.Id, StorageId = entity.CurrentStorageId };
        }

        [HttpPost("Create")]
        public async Task<CommonResponseDto> CreateAsync([FromBody] CageCarCreateInput deliverProcessCreateInput)
        {
            //todo,储位id应该检查一下。或者在数据库中建立外键。否则储位不存在都不知道？
            await this.cageCarsRepos.InsertAsync(new CageCar(deliverProcessCreateInput.CageCaId, deliverProcessCreateInput.StorageId));
            return CommonResponseDto.CreateSuccessResponse(deliverProcessCreateInput.FrameId);
        }
        [HttpPost("Update")]
        public async Task<CommonResponseDto> UpdateAsync(CageCarUpdateInput input)
        {
            var entity =await cageCarsRepos.GetAsync(m => m.Id == input.Id);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }

        [HttpPost("Delete")]
        public async Task<CommonResponseDto> DeleteAsync(CageCarDeleteInput input)
        {
            var entity = await cageCarsRepos.GetAsync(m => m.Id == input.Id);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }

        [HttpPost("GetList")]
        public async Task<List<CageCarDto>> GetListAsync()
        {

            //左联
            var Query = from cageCar in await this.cageCarsRepos.WithDetailsAsync(m=>m.Packages)
                        join storage in await this.cageCarStoragesRepos.GetQueryableAsync() on cageCar.CurrentStorageId equals storage.Id into temp

                        from g in temp.DefaultIfEmpty()
                        join sort in await this.packageSortsRepos.GetQueryableAsync() on cageCar.PackageSortId equals sort.Id into temp2

                        from g1 in temp2.DefaultIfEmpty()
                        select new { CageCar = cageCar, StroageType = g.StorageType, SortDescribtion = g1.Describe };


            var Result = new List<CageCarDto>();
            foreach(var c in Query.ToList())
            {
                Result.Add(new CageCarDto()
                {
                    Id = c.CageCar.Id,
                    CurrentStorageId = c.CageCar.CurrentStorageId,
                    CurrentStorageType = (CageCarCageCarStorageTypeDto)c.StroageType,
                    DeliverOutletId = c.CageCar.DeliverOutletId ,
                    PackageSortDescribtion = c.SortDescribtion,
                    PakageCount = c.CageCar.Packages.Count(),
                    State = (CageCarStateDto)c.CageCar.State,
                }) ;
            }
            return Result;
        }


        [HttpGet("Pagination")]
        public async Task<PagedResultDto<CageCarDto>> GetPageAsync(GetCageCarPageRequest input)
        {

            //左联
            var Query = from cageCar in (await this.cageCarsRepos.WithDetailsAsync(m=>m.Packages)).WhereIf(!string.IsNullOrEmpty(input.Code),m=>m.Id.Contains(input.Code))
                        join storage in await this.cageCarStoragesRepos.GetQueryableAsync() on cageCar.CurrentStorageId equals storage.Id into temp

                        from g in temp.DefaultIfEmpty()
                        join sort in await this.packageSortsRepos.GetQueryableAsync() on cageCar.PackageSortId equals sort.Id into temp2

                        from g1 in temp2.DefaultIfEmpty()
                        select new { CageCar = cageCar, StorageNodeName =  g.MapNodeName, StroageType = g.StorageType, SortDescribtion = g1.Describe };

            var Result = new List<CageCarDto>();
            
            var totalCount = Query.Count();
            var data = Query.OrderBy(m => m.CageCar.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            foreach (var c in data)
            {
                Result.Add(new CageCarDto()
                {
                    Id = c.CageCar.Id,
                    CurrentStorageId =c.CageCar.CurrentStorageId,
                    StorageNodeName =c.StorageNodeName,
                    CurrentStorageType = (CageCarCageCarStorageTypeDto)c.StroageType,
                    DeliverOutletId = c.CageCar.DeliverOutletId,
                    PackageSortDescribtion = c.SortDescribtion,
                    PakageCount = c.CageCar.Packages.Count(),
                    State = (CageCarStateDto)c.CageCar.State,
                });
            }
            return new PagedResultDto<CageCarDto>()
            {
                TotalCount = totalCount,
                Items = Result
            };
        }

    }

    



}
