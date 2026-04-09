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
using Volo.Abp.Threading;
using Volo.Abp.Application.Dtos;
using PDS.Dtos;



//TODO:笼车的状态还没有处理。暂时可以不管。

//TODO:appservice应该只写业务相关的，数据查询、统计啥的，不应该写。
//TODO:怎么让appservice不生成api呢？现在如果不定义Route等api元素，会导致swagger报错，导致其他的也看不了。
//TODO:对于特殊关联其他表列的并发，应该如果合理。 例如查找表A中的元素，如果A有没有数据，则添加B表中一个新数据，这个数据要求是确保A不会增加。
//TODO:很多还没考虑并发异常。
//totest:异步和UnitOfWork
//todo：必须设置异常投递口，否则遇到包裹无法确定投递分类的的时候，会一直溜车。
//TODO:对于同步事件，时间处理中往往需要查询关联信息，但有可能这个关联信息是在事件前面的代码增删改的，那么查询的结果或缺少事件前的内容，导致问题。难道每一个函数都需要SaveChage（)?还是说事件都用local？现在好像已经没有local了？


namespace PDS
{
    //[RemoteService(false)]
    [Route("PDS/api/OperationStation")]
    public class OperationStationAppService : PDSAppService, IOperationStationAppService   //接口名称必须等于“I”加类名称，否则不自动注入。
    {
        private readonly ILogger<OperationStationAppService> logger;
        private readonly IRepository<OperationStation, string> operationStationRepository;
        private readonly IRepository<Package, string> packagesRepository;
        private readonly IRepository<DeliverProcessTask, string> deliverProcessRepository;

        public OperationStationAppService(
            ILogger<OperationStationAppService> logger,
            IRepository<OperationStation, string> OperationStationRepository,
            IRepository<Package, string> packagesRepository,
            IRepository<DeliverProcessTask, string> deliverProcessRepository
            )
        {
            this.logger = logger;
            this.operationStationRepository = OperationStationRepository;
            this.packagesRepository = packagesRepository;
            this.deliverProcessRepository = deliverProcessRepository;
        }

        /// <summary>
        /// 创建操作点
        /// </summary>
        /// <param name="opStationCreateInput"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Creat")]
        public async Task<CommonResponseDto> CreatAsync(OpStationCreateInput opStationCreateInput)
        {
            var NewOpStation = new OperationStation(opStationCreateInput.OperationStationId, opStationCreateInput.BindingMapName, opStationCreateInput.MaxAgvCount, opStationCreateInput.loadRobotId);
            try
            {
                await this.operationStationRepository.InsertAsync(NewOpStation);
                return CommonResponseDto.CreateSuccessResponse(opStationCreateInput.FrameId);
            }
            catch (SqlException e)
            {
                return CommonResponseDto.CreateFaultResponse(opStationCreateInput.FrameId, e.Message);
            }
        }
        [HttpPost]
        [Route("Update")]
        public async Task<CommonResponseDto> UpdateAsync(OpStationUpdateInput input)
        {
            var entity = await operationStationRepository.GetAsync(m => m.Id == input.OperationStationId);
            //ObjectMapper.Map<OpStationUpdateInput, OperationStation>(input);
            entity.SetMaxAgvCount(input.MaxAgvCount);
            await operationStationRepository.UpdateAsync(entity);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<CommonResponseDto> DeleteAsync(OpStationDeleteInput input)
        {
            await operationStationRepository.DeleteAsync(input.Id);
            return CommonResponseDto.CreateSuccessResponse(input.FrameId);
        }


        [HttpPost]
        [Route("OpStationOnLineSignal")]
        public async Task<CommonResponseDto> OpStationOnLineSignal(OpStationOnLineInput opStationOnLineInput)
        {
            var OpStation = await this.operationStationRepository.FindAsync(opStationOnLineInput.OperationStationId);
            if (OpStation == null)
            {
                return CommonResponseDto.CreateFaultResponse(opStationOnLineInput.FrameId, "OpStationNotFind");
            }
            else
            {
                OpStation.ClaimState(OperationStationState.ONLINE);  //设置状态为上线
                OpStation.LastSignalTime = DateTime.Now;  //更新最后一次时间。
                await this.operationStationRepository.UpdateAsync(OpStation);
                return CommonResponseDto.CreateSuccessResponse(opStationOnLineInput.FrameId);
            }
        }

        [RemoteService(false)]
        [HttpPost]
        [Route("OpStationOnLineTimeOutCheck")]
        public async Task OpStationOnLineTimeOutCheck()
        {
            var OpStations = await this.operationStationRepository.GetListAsync();
            foreach (var station in OpStations)
            {
                try
                {
                    if (station.State == OperationStationState.ONLINE &&
                        (!station.LastSignalTime.HasValue || (DateTime.Now - station.LastSignalTime.Value > TimeSpan.FromSeconds(30))))
                    {
                        station.ClaimState(OperationStationState.OUTLINE);  //设置状态为上线
                        await this.operationStationRepository.UpdateAsync(station);
                    }
                }
                catch (AbpDbConcurrencyException) { }
            }
        }
        /// <summary>
        /// 分页获取
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("Pagination")]
        public async Task<PagedResultDto<OperationStationDto>> GetPaginationList(GetOperationStaionPageRequest input)
        {
            var query = (await operationStationRepository.GetQueryableAsync()).WhereIf(!string.IsNullOrEmpty(input.Code), m => m.Id.Contains(input.Code));
            var totalCount = query.Count();
            var items = query
                .OrderBy(m => m.Id)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount).ToList();
            var resultItems = ObjectMapper.Map<List<OperationStation>, List<OperationStationDto>>(items);
            return new PagedResultDto<OperationStationDto>
            {
                TotalCount = totalCount,
                Items = resultItems
            };
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<GetOperaionStationResultDto> GetListAsync(GetStationsRequestDto input)
        {

            //分组统计，接着联表查询。  todo：如果要统计各个站点上线以来的数据，则不能分组再联表，性能很差。所以这里统计的是当天的数据。
            var task = await this.deliverProcessRepository.GetQueryableAsync();
            var OpStations = (await operationStationRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrEmpty(input.Id), m => m.Id.Contains(input.Id))
                .WhereIf(input.State.HasValue, m => m.State == input.State);
            var ops = OpStations.ToList();

            //先把task表分组统计，存入中间表
            var temp = from p in task
                       where p.PackageLoadTime.HasValue && p.PackageLoadTime.Value.CompareTo(DateTime.Now.Date) > 0 //已经投递,且时间为当天。
                       group p by p.OperationStationId into g  //分组，记录为g
                       select new { OperationStationId = g.Key, Total = g.Count() };  //从分组结果中获取到统计数量

            ////内联
            //var q3 = from p in OpStations  
            //         join q in q2
            //         on p.Id equals q.OperationStationId
            //         select new { Opstation = p, StationLoadCount = q.Total };  //联表查询当前所有的站点，所有站点都要有统计数据。

            //左联
            var q4 = from p in OpStations
                     join q in temp
                     on p.Id equals q.OperationStationId into g  //内联结果放入temp队列中
                     from w in g.DefaultIfEmpty()
                     select new { Opstation = p, StationLoadCount = (int?)w.Total };    //联表查询，的出来的结果可能是空，所以必须用一个可控的类型来存储，否则报错。

            var OpStationDtos = new List<OperationStationDto>();
            var data = q4.ToList();
            foreach (var o in data)
            {
                OpStationDtos.Add(new OperationStationDto()
                {
                    Id = o.Opstation.Id,
                    SignInTime = o.Opstation.SignInTime.ToString(),
                    MapNodeName = o.Opstation.MapNodeName,
                    PackageCount = (o.StationLoadCount == null ? 0 : o.StationLoadCount.Value),
                    State = o.Opstation.State.ToString(),
                    UserId = o.Opstation.LoadRobotId,   //对于设备上线，上线信息指向上料设备。
                    UserName = o.Opstation.LoadRobotId,
                });
            }
            return new GetOperaionStationResultDto()
            {
                TotalCount = OpStationDtos.Count,
                Items = OpStationDtos
            };
        }


        [HttpPost]
        [Route("GetPackageCountStatistics")]
        public async Task<List<HourPackStatisticsDto>> GetPackageCountStatisticsAsync([FromBody] OpStationTodayPackageStatisticsInput opStationTodayPackageStatisticsInput)
        {
            var query = from p in await this.deliverProcessRepository.GetQueryableAsync()
                        where p.PackageLoadTime.HasValue && p.PackageLoadTime.Value.CompareTo(DateTime.Now.Date) > 0 //找出指定的数据
                        group p by p.PackageLoadTime.Value.Hour into p2  //根据hour
                        select new { Hour = p2.Key, Count = p2.Count() };

            var result = new List<HourPackStatisticsDto>();

            foreach (var t in query.ToList())
            {
                result.Add(new HourPackStatisticsDto() { Hour = t.Hour, PackageCount = t.Count });
            }
            return result;
        }

        /// <summary>
        /// 统计状态数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("Statistics")]
        public async Task<List<GetStationStatisticsDto>> GetStationStatistics()
        {
            var statistics = from station in await operationStationRepository.GetQueryableAsync()
                             group station by station.State into g
                             select new GetStationStatisticsDto()
                             {
                                 State = g.Key.ToString(),
                                 Quantity = g.Count()
                             };
            return statistics.ToList();
        }

        /// <summary>
        /// 查询最近供货量
        /// </summary>
        /// <param name="intervalHour"></param>
        /// <returns></returns>
        [HttpGet("StatisticsByHour")]
        public async Task<List<HourPackStatisticsDto>> GetStationPackageCountStatisticsByHourAsync(string stationId, int intervalHour)
        {
            var results = new List<HourPackStatisticsDto>();
            var originalTime = DateTime.Now.AddHours(-intervalHour);
            for (var i = 0; i < intervalHour; i++)
            {
                var statisticsTime = originalTime.AddHours(i);
                results.Add(new HourPackStatisticsDto()
                {
                    Day = statisticsTime.Day,
                    Hour = statisticsTime.Hour,
                });
            }
            var packageRecords = (await deliverProcessRepository.GetQueryableAsync()).Where(m => m.CreationTime.HasValue)
                        .WhereIf(!string.IsNullOrEmpty(stationId), m => m.OperationStationId == stationId)
                        .WhereIf(intervalHour != 0, m => m.CreationTime.Value > originalTime);
            var query = from station in packageRecords
                        group station by new
                        {
                            station.CreationTime.Value.Hour,
                            station.CreationTime.Value.Date,
                        }
                        into g
                        orderby g.Key.Date
                        orderby g.Key.Hour
                        select new HourPackStatisticsDto
                        {
                            Day = g.Key.Date.Day,
                            Hour = g.Key.Hour,
                            PackageCount = g.Count()
                        };

            var queryResult = query.ToList();
            foreach (var result in results)
            {
                var value = queryResult.FirstOrDefault(m => m.Day == result.Day && m.Hour == result.Hour);
                if (value != null)
                    result.PackageCount = value.PackageCount;
            }
            return results;
        }
        [HttpGet("StatisticsAllByHour")]
        public async Task<List<StationsHourPackageStaticDto>> GetAllPackageCountStatisByHour(int intervalHour)
        {
            var result = new List<StationsHourPackageStaticDto>();
            var startTime = DateTime.Now.AddHours(-(intervalHour-1));
            //将异步查询变同步
            var stations = await operationStationRepository.GetListAsync();

            var query = (from process in (await deliverProcessRepository.GetQueryableAsync()).Where(m => m.CreationTime.HasValue && m.CreationTime.Value > startTime)
                         group process by new { process.OperationStationId, process.CreationTime.Value.Date, process.CreationTime.Value.Hour } into g
                         orderby g.Key.Date
                         orderby g.Key.Hour
                         select new
                         {
                             Station = g.Key.OperationStationId,
                             Date = g.Key.Date,
                             Hour = g.Key.Hour,
                             Quantity = g.Count()
                         }).ToList();
            foreach (var station in stations)
            {
                var stationQuery = query.Where(m => m.Station == station.Id)
                                   .OrderBy(m => m.Date)
                                   .ThenBy(m => m.Hour)
                                   .ToList();
                var queryResults = new List<HourPackStatisticsDto>();
                for (var i = 0; i < intervalHour; i++)
                {
                    var statisticsTime = startTime.AddHours(i);
                    var packageData = new HourPackStatisticsDto()
                    {
                        Day = statisticsTime.Day,
                        Hour = statisticsTime.Hour
                    };

                    var data = stationQuery.FirstOrDefault(m => m.Date == statisticsTime.Date && m.Hour == statisticsTime.Hour);
                    if (data != null)
                        packageData.PackageCount = data.Quantity;
                    queryResults.Add(packageData);
                }
                result.Add(new StationsHourPackageStaticDto
                {
                    Station = station.Id,
                    Data = queryResults
                });
            }
            return result;
        }

        [HttpPost]
        [Route("UpdateMaxAgvLineCount")]
        public async Task<CommonResponseDto> UpdateMaxAgvLineCountAsync([FromBody] OpStationUpdateAgvCountInput opStationUpdateAgvCountInput)
        {
            var OpStation = await this.operationStationRepository.FindAsync(opStationUpdateAgvCountInput.OperationStationId);
            if (OpStation == null)
            {
                return CommonResponseDto.CreateFaultResponse(opStationUpdateAgvCountInput.FrameId, "OpStationNotFind");
            }
            else
            {
                try
                {
                    OpStation.SetMaxAgvCount(opStationUpdateAgvCountInput.MaxAgvCount);
                    await this.operationStationRepository.UpdateAsync(OpStation);
                    return CommonResponseDto.CreateSuccessResponse(opStationUpdateAgvCountInput.FrameId);
                }
                catch (AbpDbConcurrencyException)
                {
                    return CommonResponseDto.CreateFaultResponse(opStationUpdateAgvCountInput.FrameId, "并发错误，请稍后重试！");
                }
            }
        }
    }



}
