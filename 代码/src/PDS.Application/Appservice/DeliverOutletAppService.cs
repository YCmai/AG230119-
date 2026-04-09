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
using PDS.Domain.Interface;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    [Route("PDS/api/DeliverOutlet")]
    public class DeliverOutletAppService:PDSAppService, IDeliverOutletAppService
    {
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IRepository<CageCar, string> cageCarsRepos;
        private readonly IRepository<CageCarStorage, string> cageCarStoragesRepos;
        private readonly IDeliverOutletManager deliverOutletManager;

        public DeliverOutletAppService(
            IRepository<DeliverOutlet,string> deliverOutletsRepos,
            IRepository<CageCar,string> cageCarsRepos,
            IRepository<CageCarStorage,string> cageCarStoragesRepos,
            IDeliverOutletManager deliverOutletManager
            )
        {
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.cageCarsRepos = cageCarsRepos;
            this.cageCarStoragesRepos = cageCarStoragesRepos;
            this.deliverOutletManager = deliverOutletManager;
        }

        [HttpPost("AutoBindingNearCageCar")]
        public async Task AutoBindingNearCageCar()
        {
            var cageCars = await this.cageCarsRepos.GetQueryableAsync();
            var deliverOutlets = await this.deliverOutletsRepos.GetQueryableAsync();
            var cageCarStorages = await this.cageCarStoragesRepos.GetQueryableAsync();


            //内联
            var Query1 = from cageCar in cageCars
                         where cageCar.State == CageCarState.PACKAGE_AVAILABLE && (cageCar.DeliverOutletId == null || cageCar.DeliverOutletId=="")
                         join storage in cageCarStorages
                         on cageCar.CurrentStorageId equals storage.Id
                         select new { cageCar = cageCar, currenNodeName = storage.MapNodeName };


            //内联
            var Query = from deliverOut in deliverOutlets
                        where deliverOut.State == DeliverOutletState.FREE
                        join cagecarInfo in Query1
                        on deliverOut.DownMapMapNodeName equals cagecarInfo.currenNodeName
                        select new { deliverOutlet = deliverOut, cageCar = cagecarInfo.cageCar };
            var data = Query.ToList();
            foreach(var q in data)
            {
                deliverOutletManager.Binding(q.deliverOutlet, q.cageCar);
                await this.cageCarsRepos.UpdateAsync(q.cageCar);
                await this.deliverOutletsRepos.UpdateAsync(q.deliverOutlet);
            }
        }

        [HttpPost("Create")]
        public async Task<CommonResponseDto> CreateAsync([FromBody]DeliverOutletCreateInput deliverProcessCreateInput)
        {
            await this.deliverOutletsRepos.InsertAsync(
                new DeliverOutlet(
                deliverProcessCreateInput.DeliverOutletId,
                deliverProcessCreateInput.DeliverOutletType,
                deliverProcessCreateInput.UppperMapNodeName,
                deliverProcessCreateInput.DownMapMapNodeName,
                deliverProcessCreateInput.PackageSortid
                ));
            return CommonResponseDto.CreateSuccessResponse(deliverProcessCreateInput.FrameId);
        }
        [HttpPost("Update")]
        public async Task<CommonResponseDto> UpdateAsync(DeliverOutletUpdateInput input)
        {
            var entity = await deliverOutletsRepos.GetAsync(m => m.Id == input.Id);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }
        [HttpPost("Delete")]
        public async Task<CommonResponseDto> DeleteAsync(DeliverOutletDeleteInput input)
        {
            var entity = await deliverOutletsRepos.GetAsync(m => m.Id == input.Id);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }

        [HttpGet("GetList")]
        public async Task<DeliverOutletGetListOutput> GetList(DeliverOutletGetListInput dto)
        {
            var cageCars = await this.cageCarsRepos.WithDetailsAsync(m=>m.Packages);
            var deliverOutlets = (await this.deliverOutletsRepos.WithDetailsAsync())
                .WhereIf(!string.IsNullOrEmpty(dto.Id), t => t.Id.Contains(dto.Id))
                .WhereIf(dto.State.HasValue, t => t.State == dto.State)
                .WhereIf(dto.DeliverType.HasValue, t => t.DeliverType == dto.DeliverType);

            //左联
            var Query = (from deliverOut in deliverOutlets
                        join cageCar in cageCars
                        on deliverOut.CageCarId equals cageCar.Id into g
                        from w in g.DefaultIfEmpty()
                        select new { deliverOutlet = deliverOut, cageCars = w }).ToList();

            List<DeliverOutletDto> Result = new List<DeliverOutletDto>();
            foreach (var temp in Query)
            {
                Result.Add(new DeliverOutletDto()
                {
                    CageCarId = temp.deliverOutlet.CageCarId,
                    DeliverType = temp.deliverOutlet.DeliverType.ToString(),
                    DownMapMapNodeName = temp.deliverOutlet.DownMapMapNodeName,
                    UppperMapNodeName = temp.deliverOutlet.UppperMapNodeName,
                    Id = temp.deliverOutlet.Id,
                    PackageCount = temp.cageCars?.Packages == null ? 0 : temp.cageCars.Packages.Count,
                    State = temp.deliverOutlet.State.ToString(),
                    OnDeliverCount = temp.deliverOutlet.HandlingProcess == null ? 0 : temp.deliverOutlet.HandlingProcess.Count,
                });
            }

            return new DeliverOutletGetListOutput() { TotalCount = Result.Count, Items = Result };
        }


        [HttpPost("ClearPackages")]
        public async Task<CommonResponseDto> ClearPackagesAsync(ClearPackagesInput clearPackagesInput)
        {

            //当前接口约定只有锁定的才可以清空。
            //获取对应投递口；
            //获取对应笼车；
            //解除这两个的绑定（规定了必须不绑定才可以清包裹记录）；
            //清除笼车；
            //重新绑定；

            var outlet =await this.deliverOutletsRepos.GetAsync(clearPackagesInput.DeliverOutetId);
            if(outlet.State == DeliverOutletState.LOCKED)
            {
                var cagecar = (await this.cageCarsRepos.WithDetailsAsync(p => p.Packages)).FirstOrDefault(p => p.Id == outlet.CageCarId);
                this.deliverOutletManager.ReleaseBinding(outlet, cagecar);
                cagecar.ClearPackageAndResetSort();
                this.deliverOutletManager.Binding(outlet, cagecar);
                return CommonResponseDto.CreateSuccessResponse(clearPackagesInput.FrameId);
            }
            return CommonResponseDto.CreateFaultResponse(clearPackagesInput.FrameId, "投递口未锁定，不能清空对应笼车的物料");
        }

        /// <summary>
        /// 获取投递口根据状态和类型的汇总信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSummary")]
        public async Task<List<DeliverOutletSummaryDto>> GetListSummary()
        {
            var stateSummary = (from deliver in await deliverOutletsRepos.GetQueryableAsync()
                                group deliver by deliver.State into g
                                select new DeliverOutletSummaryDto
                                {
                                    Type = SummaryType.STATE,
                                    Name = g.Key.ToString(),
                                    Value = g.Count()
                                }).ToList();
            var typeSummary = (from deliver in await deliverOutletsRepos.GetQueryableAsync()
                               group deliver by deliver.DeliverType into g
                               select new DeliverOutletSummaryDto
                               {
                                   Type = SummaryType.TYPE,
                                   Name = g.Key.ToString(),
                                   Value = g.Count()
                               }).ToList();
            var resultSummary = stateSummary.Union(typeSummary).ToList();
            return resultSummary;
        }
        /// <summary>
        /// 获取格口总数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("TotalCount")]
        public async Task<GetPackageOutletTotal> GetTotalCount()
        {
            var totalCount = await deliverOutletsRepos.GetCountAsync();
            return new GetPackageOutletTotal() { TotalCount = totalCount };
        }
        /// <summary>
        /// 分页获取格口数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("Pagination")]
        public async Task<PagedResultDto<DeliverOutletDto>> GetPagination(GetDeliverOutletPageRequest input)
        {
            var query = (await deliverOutletsRepos.GetQueryableAsync()).WhereIf(!string.IsNullOrEmpty(input.Code), m => m.Id.Contains(input.Code));
            var totalCount = query.Count();
            var items = query
                .OrderBy(m => m.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            var resultItems = ObjectMapper.Map<List<DeliverOutlet>, List<DeliverOutletDto>>(items);
            return new PagedResultDto<DeliverOutletDto>()
            {
                TotalCount = totalCount,
                Items = resultItems
            };
        }

    }

    



}
