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
    [Route("PDS/api/PDSSim")]
    public class PDSSimAppService : PDSAppService
    {
        private readonly ILogger<OperationStationAppService> logger;
        private readonly IRepository<OperationStation, string> operationStationsRepos;
        private readonly IRepository<Package, string> packagesRepos;
        private readonly IRepository<DeliverProcessTask, string> deliverProcessRepos;
        private readonly IRepository<PackageLoadrobotTask, string> packageLoadrobotTasksRepos;
        private readonly IRepository<PackageSort, string> packageSortsRepos;
        private readonly IPackageSortMatcher packageSortMatcher;

        public PDSSimAppService(
            ILogger<OperationStationAppService> logger,
            IRepository<OperationStation, string> OperationStationRepository,
            IRepository<Package, string> packagesRepository,
            IRepository<DeliverProcessTask, string> deliverProcessRepository,
            IRepository<PackageLoadrobotTask, string> packageLoadrobotTasksRepos,
            IRepository<PackageSort,string> packageSortsRepos,
            IPackageSortMatcher packageSortController
            )
        {
            this.logger = logger;
            this.operationStationsRepos = OperationStationRepository;
            this.packagesRepos = packagesRepository;
            this.deliverProcessRepos = deliverProcessRepository;
            this.packageLoadrobotTasksRepos = packageLoadrobotTasksRepos;
            this.packageSortsRepos = packageSortsRepos;
            this.packageSortMatcher = packageSortController;
        }



        /// <summary>
        /// 仿真中，简化了流程，一个api表示上线，有书通知（还书机->PDS通知）和滚动任务的下（PDS->还书机）
        /// </summary>
        /// <param name="deliverProcessCreateInput"></param>
        /// <returns></returns>

        [HttpPost("GetOrCreateForSim")]
        public async Task<GetOrCreateProcessDto> GetOrCreateForSimAsync([FromBody] PDSSimProcessInput pDSSimProcessInput)
        {
            logger.LogDebug("GetOrCreateForSimAsync:" + JsonConvert.SerializeObject(pDSSimProcessInput));
            //触发了op站点上线
            //return new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
            //return null;
            var OpStation =await this.operationStationsRepos.FindAsync(pDSSimProcessInput.OperationStationId);
            //System.Diagnostics.Debug.WriteLine("触发：" + pDSSimProcessInput.OperationStationId);
            if (OpStation == null) throw new Exception("OpStationNotFind");
            try
            {
                OpStation.ClaimState(OperationStationState.ONLINE);  //设置状态为上线
                OpStation.LastSignalTime = DateTime.Now;  //更新最后一次时间。
                await this.operationStationsRepos.UpdateAsync(OpStation);
            }
            catch(Exception ex)
            {

            }

            GetOrCreateProcessDto result = null;
            switch (pDSSimProcessInput.State)
            {
                case BookState.NoBook:
                    //没有书本，也可能是书本已经放置完。
                    result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                    logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                    return result;
                    break;

                case BookState.HasUnhadleBook:

                    #region 检查有没有相关记录，没有则添加，有则看看流程是否去到还书机投书关节，是则需要回复还书机要投书了。
                   
                    var Process = await this.deliverProcessRepos.FindAsync(p => p.PackageId == pDSSimProcessInput.PackageCode);
                    //查看之前是否已经有了这本书的记录
                    if (Process != null)
                    {
                        //已有，则返回告诉还书机要不要放置书本。
                        var LoadTask =await this.packageLoadrobotTasksRepos.FindAsync(p => p.DeliverProcessId == Process.Id && p.TaskState == PackageLoadrobotTaskState.NotStart);
                        if (LoadTask == null)
                        {
                            //还没有这个放置书本的步骤，先不用管。
                            result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                            logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                            return result;
                        }
                        else
                        {
                            //需要投书了。
                            result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = true };
                            logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                            return result;
                        }
                    }
                    else  //还没有这个书本的相关记录，则添加：
                    {
                        //添加前看看之前是否有这个操作点的未“放书”的任务。如果是，表示这个还书机上一个书还没有放书到gav，就变成另一个书了
                        //认为上一本书已经取消了。（todo：是认为取消，还是认为上一本书已经投递完成？）
                        var UnhandleOldBookProcess = await this.deliverProcessRepos.FindAsync(p =>
                    p.OperationStationId == pDSSimProcessInput.OperationStationId
                    && p.PackageId != pDSSimProcessInput.PackageCode
                    && (p.DetailState== DeliverProcessState.WAITING_AGV || p.DetailState == DeliverProcessState.LOADING_PACKING));
                        if (UnhandleOldBookProcess != null)
                        {
                            //先取消之前的工作流。
                            UnhandleOldBookProcess.ClaimDetailState(DeliverProcessState.DELIVER_FAULT);

                            //取消agv的占用（如果已经占用）。todo：比较难判断，因为存在延迟。

                            //取消之前的放书任务（如果有）。

                            //比较好的方案应该是添加一个流程，专门处理本流程的取消。例如添加一个
                            logger.LogError("出现未完成的Process" + JsonConvert.SerializeObject(UnhandleOldBookProcess));
                            result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                            logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                            return result;
                        }

                        else  //原来没有未处理的书，才可以添加新的书本。
                        {

                            PackageSort SortForNewPackage;
                            //确定包裹的类型
                            if (pDSSimProcessInput.PackageSortDecribtion.IsNullOrWhiteSpace())
                            {
                                SortForNewPackage = await packageSortMatcher.MatchAsync(pDSSimProcessInput.PackageCode);
                            }
                            else
                            {
                                SortForNewPackage = await this.packageSortsRepos.FindAsync(p => p.Describe.Equals(pDSSimProcessInput.PackageSortDecribtion));
                            }
                            if (SortForNewPackage == null) SortForNewPackage = await this.packageSortsRepos.GetAsync(p => p.Describe.Equals("Unknown"));


                            //添加包裹
                            var NewPackage = new Package(pDSSimProcessInput.PackageCode, pDSSimProcessInput.PackageCode, SortForNewPackage.Id);
                            await this.packagesRepos.InsertAsync(NewPackage);


                            //创建投递流程
                            var NewProcess = new DeliverProcessTask(Guid.NewGuid().ToString(), NewPackage.Id, pDSSimProcessInput.OperationStationId);
                            await this.deliverProcessRepos.InsertAsync(NewProcess);

                            result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                            logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                            return result;
                        }
                    }

                    #endregion
                    break;
   

                case BookState.PuttingBook:

                    result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                    logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                    return result;
                    break;

                default:
                    result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
                    logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
                    return result;
                    break;
            }
            result = new GetOrCreateProcessDto() { FrameId = pDSSimProcessInput.FrameId, PutBook = false };
            logger.LogDebug("GetOrCreateForSimAsync Result:" + JsonConvert.SerializeObject(result));
            return result;
        }

        [HttpPost("PutBookTaskFinished")]
        /// <summary>
        /// 当工作站完成放书任务后调用。这里假设还书机投递完书本以后会调用接口（todo：实际情况下，设备是不知道自己是否完成的，所以很可能要PDS靠状态记录来判断）
        /// </summary>
        /// <param name="pDSSimProcessInput"></param>
        /// <returns></returns>
        public async Task<CommonResponseDto> PutBookTaskFinished([FromBody] PDSSimProcessInput pDSSimProcessInput)
        {
            //因为同一个操作站同一个时间最多一个任务正在投书，因此，可以唯一找出对应的Process。
            var Process = await this.deliverProcessRepos.FindAsync(p => p.OperationStationId == pDSSimProcessInput.OperationStationId && p.DetailState == DeliverProcessState.LOADING_PACKING);
            var PutBookTask = await this.packageLoadrobotTasksRepos.FindAsync(p => p.DeliverProcessId == Process.Id);
            PutBookTask.ClaimTaskState(PackageLoadrobotTaskState.Finished);
            await this.packageLoadrobotTasksRepos.UpdateAsync(PutBookTask);
            logger.LogInformation("PutBookTaskFinished:" + JsonConvert.SerializeObject(pDSSimProcessInput));
            var Result = CommonResponseDto.CreateSuccessResponse(pDSSimProcessInput.FrameId);
            logger.LogInformation("PutBookTaskFinished Result:" + JsonConvert.SerializeObject(Result));
            return Result;
        }
    }
}
