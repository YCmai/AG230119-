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
using Volo.Abp.Application.Dtos;
using PDS.Dtos;

namespace PDS
{

    //totest：并发
    [Route("PDS/api/DeliverProcess")]
    /// <summary>
    /// 用于实现对投递流程的控制
    /// </summary>
    public class DeliverProcessAppService : PDSAppService
    {
        protected ILogger<PDSApplicationModule> _logger;  //todo,目前如果不使用构造函数注入，这里是空。
        private readonly IRepository<DeliverProcessTask, string> _packageDeliverProcessRepos;
        private readonly IRepository<DeliverOutlet, string> deliverOutletsRepos;
        private readonly IRepository<PackageSort, string> packageSortsReps;
        private readonly IRepository<OperationStation, string> operationStationsRepos;
        private readonly IDeliverOutletAllocator deliverOutletAllocator;
        private readonly IPackageSortMatcher packageSortMatcher;
        protected IRepository<Package, string> _PackagesRepos;
        private readonly IRepository<CageCar, string> cageCarsRepos;

        public DeliverProcessAppService(ILogger<PDSApplicationModule> logger,
            IRepository<Package, string> PackagesRepos,
            IRepository<DeliverProcessTask, string> packageDeliverProcessRepos,
            IRepository<DeliverOutlet, string> deliverOutletsRepos,
            IRepository<PackageSort, string> packageSortsReps,
            IRepository<OperationStation, string> operationStationsRepos,
            IRepository<CageCar, string> cageCarsRepos,
            IDeliverOutletAllocator deliverOutletAllocator,
            IPackageSortMatcher packageSortController
            )
        {
            this._logger = logger;
            this._PackagesRepos = PackagesRepos;
            this._packageDeliverProcessRepos = packageDeliverProcessRepos;
            this.deliverOutletsRepos = deliverOutletsRepos;
            this.packageSortsReps = packageSortsReps;
            this.operationStationsRepos = operationStationsRepos;
            this.deliverOutletAllocator = deliverOutletAllocator;
            this.packageSortMatcher = packageSortController;
            this.cageCarsRepos = cageCarsRepos;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<CommonResponseDto> CreateAsync(DeliverProcessCreateInput deliverProcessCreateInput)
        {
            PackageSort SortForNewPackage;
            //确定包裹的类型
            if (deliverProcessCreateInput.PackageSortDecribtion.IsNullOrWhiteSpace())
            {
                SortForNewPackage = await packageSortMatcher.MatchAsync(deliverProcessCreateInput.PackageCode);
            }
            else
            {
                SortForNewPackage = await this.packageSortsReps.FindAsync(p => p.Describe.Equals(deliverProcessCreateInput));
            }
            if (SortForNewPackage == null) SortForNewPackage = await this.packageSortsReps.GetAsync(p => p.Describe.Equals("Unknown"));

            //添加包裹
            var NewPackage = new Package(deliverProcessCreateInput.PackageCode, deliverProcessCreateInput.PackageSortDecribtion, SortForNewPackage.Id);
            await _PackagesRepos.InsertAsync(NewPackage);


            //创建投递流程
            var NewProcess = new DeliverProcessTask(Guid.NewGuid().ToString(), NewPackage.Id, deliverProcessCreateInput.OperationStationId);
            await _packageDeliverProcessRepos.InsertAsync(NewProcess);

            return CommonResponseDto.CreateSuccessResponse(deliverProcessCreateInput.FrameId);
        }
        /// <summary>
        /// 根据投递口ID获取投递记录
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetByOutlet")]
        public async Task<PagedResultDto<GetDeliverProcessDto>> GetDeliverProcessByOutletId(GetDeliverProcessPageRequest input)
        {

            var cageCar = (await cageCarsRepos.WithDetailsAsync(m=>m.Packages)).FirstOrDefault(m => m.DeliverOutletId == input.DeliverOutletId);
            if (cageCar == null)
            {
                return new PagedResultDto<GetDeliverProcessDto>()
                {
                    TotalCount = 0,
                    Items = new List<GetDeliverProcessDto>()
                };
            };
            var totalCount = cageCar.Packages.Count;
            var processRecords=from link in cageCar.Packages
                               join package in await _packageDeliverProcessRepos.GetQueryableAsync()
                               on link.PackageId equals package.PackageId
                               select new GetDeliverProcessDto
                               {
                                   Id = package.Id,
                                   Station = package.OperationStationId,
                                   PackageId = package.PackageId,
                                   CreationTime = package.CreationTime,
                                   CloseTime = package.CloseTime
                               };
            var items = processRecords.OrderBy(m => m.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<GetDeliverProcessDto>()
            {
                TotalCount = totalCount,
                Items = items
            };
            //var processRecords = from process in await _packageDeliverProcessRepos.GetQueryableAsync()
            //                     where process.CageCarId == cageCar.Id
            //select new GetDeliverProcessDto
            //{
            //    Id = process.Id,
            //    Station = process.OperationStationId,
            //    PackageId = process.PackageId,
            //    CreationTime = process.CreationTime,
            //    CloseTime = process.CloseTime
            //};
            //var totalCount = processRecords.Count();
            //var items = processRecords.OrderBy(m => m.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            //return new PagedResultDto<GetDeliverProcessDto>()
            //{
            //    TotalCount = totalCount,
            //    Items = items
            //};
        }
        /// <summary>
        /// 历史数量汇总
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSummaryCount")]
        public async Task<GetPackageProcessSummaryCount> GetPackageProcessSummaryCount()
        {
            //var total = await _packageDeliverProcessRepos.GetCountAsync();
            var sumGroupValues = from package in await _packageDeliverProcessRepos.GetQueryableAsync()
                                 join outlet in await deliverOutletsRepos.GetQueryableAsync()
                                 on package.DeliverOutletId equals outlet.Id
                                 group package by outlet.DeliverType into g
                                 select new SumPackageProcessGroupValue
                                 {
                                     Type = g.Key,
                                     Count = g.Count()
                                 };
            var result = new GetPackageProcessSummaryCount();
            foreach (var value in sumGroupValues)
            {

                if (value.Type == DeliverOutletType.ABNORMAL)
                    result.AbnormalCount = value.Count;

                if (value.Type == DeliverOutletType.NORMAL)
                    result.NormalCount = value.Count;
            }
            return result;
        }
        /// <summary>
        /// 当天数量汇总
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSummaryCountByHour")]
        public async  Task<GetPackageProcessSummaryCount> GetPackageProcessSummaryCountByHours(int intervalHours)
        {
            var earliestTime = DateTime.Now.Date;
            var sumGroupValues = from package in (await _packageDeliverProcessRepos.GetQueryableAsync()).Where(m => m.CreationTime > earliestTime)
                                 join outlet in await deliverOutletsRepos.GetQueryableAsync()
                                 on package.DeliverOutletId equals outlet.Id
                                 group package by outlet.DeliverType into g
                                 select new SumPackageProcessGroupValue
                                 {
                                     Type = g.Key,
                                     Count = g.Count()
                                 };
            var result = new GetPackageProcessSummaryCount();
            foreach (var value in sumGroupValues)
            {
                if (value.Type == DeliverOutletType.ABNORMAL)
                    result.AbnormalCount = value.Count;
                if (value.Type == DeliverOutletType.NORMAL)
                    result.NormalCount = value.Count;
            }
            return result;
        }
        /// <summary>
        /// 根据分类进行汇总
        /// </summary>
        /// <param name="intervalHour"></param>
        /// <returns></returns>
        [HttpGet("SummaryBySorts")]
        public async Task<List<StatisticsPackageProcessBySort>> GetPackageProcessSummaryBySorts(int intervalHours)
        {
            var startTime = DateTime.Now.AddHours(-intervalHours);
            var query = (from process in (await _packageDeliverProcessRepos.GetQueryableAsync()).Where(m => m.CreationTime.HasValue && m.CreationTime > startTime)
                         join outlet in await deliverOutletsRepos.GetQueryableAsync()
                         on process.DeliverOutletId equals outlet.Id
                         join sort in packageSortsReps.AsQueryable()
                         on outlet.PackageSortId equals sort.Id
                         group process by sort.Describe into g
                         select new StatisticsPackageProcessBySort
                         {
                             SortName = string.IsNullOrEmpty(g.Key) ? "" : g.Key,
                             PackageCount = g.Count()
                         }).ToList();
            //获取前4个
            var topSorts = query.Where(m => m.PackageCount > 0).OrderBy(m => m.PackageCount).Take(4).ToList();
            var otherSorts = query.Where(m => !topSorts.Contains(m)).Sum(m => m.PackageCount);
            if (otherSorts > 0)
                topSorts.Add(new StatisticsPackageProcessBySort() { SortName = "其他", PackageCount = otherSorts });
            return topSorts;
        }
        /// <summary>
        /// 按小时统计包裹数量
        /// </summary>
        /// <param name="intervalHours"></param>
        /// <returns></returns>
        [HttpGet("DayCount")]
        public async Task<List<PackageProcessDayCount>> GetPackageProcessDayCounts(int intervalDays)
        {
            var result = new List<PackageProcessDayCount>();
            var startTime = DateTime.Now.Date.AddDays(-(intervalDays - 1));

            var totalQuery = from process in (await _packageDeliverProcessRepos.GetQueryableAsync()).Where(m => m.CreationTime.HasValue && m.CreationTime.Value > startTime)
                             group process by new { process.CreationTime.Value.Date } into g
                             orderby g.Key.Date
                             select new PackageProcessDayCount()
                             {
                                 Date = g.Key.Date,
                                 PackageCount = g.Count()
                             };
            var totalResults = totalQuery.ToList();
            for (var i = 0; i < intervalDays; i++)
            {
                var time = startTime.AddDays(i);
                var totalResult = new PackageProcessDayCount()
                {
                    Date = time.Date
                };
                var value = totalResults.FirstOrDefault(m => m.Date == totalResult.Date);
                if (value != null)
                    totalResult.PackageCount = value.PackageCount;
                result.Add(totalResult);
            }
            return result;
        }
        /// <summary>
        /// 分页获取
        /// </summary>
        /// <returns></returns>
        [HttpGet("Pagination")]
        public async Task<PagedResultDto<PackageProcessDto>> GetPagintion(PackageProcessPageRequest input)
        {
            var query = (await _packageDeliverProcessRepos.GetQueryableAsync())
                .WhereIf(!string.IsNullOrEmpty(input.Code), m => m.OperationStationId.Contains(input.Code) || m.PackageId.Contains(input.Code));
            var totalCount = query.Count();
            var items = query
                .OrderByDescending(m => m.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            var resultItems = ObjectMapper.Map<List<DeliverProcessTask>, List<PackageProcessDto>>(items);
            return new PagedResultDto<PackageProcessDto>()
            {
                TotalCount = totalCount,
                Items = resultItems
            };
        }

        internal class SumPackageProcessGroupValue
        {
            /// <summary>
            /// 投递口类型
            /// </summary>
            public DeliverOutletType Type { get; set; }
            /// <summary>
            /// 数量
            /// </summary>
            public int Count { get; set; }
        }

    }



}
