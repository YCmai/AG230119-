using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PDS.Application.Controcts;
using PDS.Domain.Entitys;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace PDS.Application.Worker
{

    /// <summary>
    /// 负责分配任务的同步和更新
    /// </summary>
    public class HBTaskSyncWorker : RepeatBackgroundWorkerBase, Volo.Abp.DependencyInjection.ITransientDependency
    {

        private string HBUrl = "http://10.10.40.33:7005";
        const string AllocationUrl = "/api/PDS/CallAgvDeliveryRequest";
        const string MoveUrl =  "/api/PDS/MoveAndWaitDeliveryRequest";
        const string DeliveryRequestUrl = "/api/PDS/DeliveryRequest";
        const string GetTaskStatusUrl =  "/api/PDS/GetTaskStatus";

        private readonly IRepository<HBAgvTask, string> hBAgvTasksRepos;
        private readonly ILogger<PDSApplicationModule> logger;
        private readonly ISettingProvider settingProvider;

        public HBTaskSyncWorker(IRepository<HBAgvTask,string> hBAgvTasksRepos,ILogger<PDSApplicationModule> logger, ISettingProvider settingProvider) :base(1)
        {
            this.hBAgvTasksRepos = hBAgvTasksRepos;
            this.logger = logger;
            this.settingProvider = settingProvider;
        }


        public override async Task Execute(IJobExecutionContext context)
        {
            var hbUrl =await this.settingProvider.GetOrNullAsync(PDSSettingDefinitionProvider.HBUrl);
            if(!hbUrl.IsNullOrWhiteSpace())
            {
                this.HBUrl = hbUrl;
            }

            //return;
            //获取所有未完成的任务
            var NoFinishedTask = await this.hBAgvTasksRepos.GetListAsync((p =>
             p.TaskState == HBAgvTaskState.NotStart
             || p.TaskState == HBAgvTaskState.MovingToTarget));



            //todo，如果都是通过api来查询，查询的量会很大；必须配合主动上报机制，减小查询量；
            foreach (var t in NoFinishedTask)
            {
                //同步分配任务，等待任务完成
                if (t.TaskName == HBAgvTask.AllocationAt)
                {
                    //1.通过HB的api，查询状态；
                    var Result = HttpClientHelper.Post<TaskResponse>(HBUrl + GetTaskStatusUrl, new { TaskID = t.TaskIdInHB });

                    ////test
                    //if ((DateTime.Now - t.CreationTime).TotalMilliseconds > 5000)
                    //{
                    //    Result = new TaskResponse() { DeviceID = "101", responseCode = 0, taskStatusCode = HBTaskStatusType.Finished };
                    //}


                    if (Result == null) return;
                    //2.任务存在，则根据查询到的状态更新PDS中的记录
                    if (Result.IsSuccess())
                    {
                        switch (Result.taskStatusCode)
                        {
                            case HBTaskStatusType.Waitting:
                            case HBTaskStatusType.Working:
                                break;

                            case HBTaskStatusType.Finished:
                                t.ClaimAgv(Result.DeviceID);
                                t.ClaimTaskState(HBAgvTaskState.FinishedAndWaitting);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                            case HBTaskStatusType.Cancel:
                                t.ClaimTaskState(HBAgvTaskState.CancelAndRelease);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                        }
                    }

                    //任务不存在，则添加。
                    else
                    {
                        Result = HttpClientHelper.Post<TaskResponse>(HBUrl + AllocationUrl, new HBTaskParam()
                        {
                            agvID = t.TargetAgvId,
                            requestCode = Guid.NewGuid().ToString(),
                            startCode = "",
                            TaskId = t.TaskIdInHB,
                            isWait = !t.NeedReleaseAgv,
                            targetCode = t.TargetNodeName
                        });

                        if (!Result.IsSuccess())
                        {
                            logger.LogWarning("添加任务失败:" + Result.describe);
                        }
                    }
                }

                else if (t.TaskName == HBAgvTask.Deliver)
                {
                    //1.通过HB的api，查询状态；
                    var Result = HttpClientHelper.Post<TaskResponse>(HBUrl + GetTaskStatusUrl, new { TaskID = t.TaskIdInHB }); 


                    ////test
                    //if ((DateTime.Now - t.CreationTime).TotalMilliseconds > 5000)
                    //{
                    //    Result = new TaskResponse() { DeviceID = "101", responseCode = 0, taskStatusCode = HBTaskStatusType.Finished };
                    //}

                    //2.任务存在，则根据查询到的状态更新PDS中的记录
                    if (Result.IsSuccess())
                    {
                        switch (Result.taskStatusCode)
                        {
                            case HBTaskStatusType.Waitting:
                            case HBTaskStatusType.Working:
                                break;

                            case HBTaskStatusType.Finished:
                                t.ClaimAgv(Result.DeviceID);
                                t.ClaimTaskState(HBAgvTaskState.FinishedAndRelease);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                            case HBTaskStatusType.Cancel:
                                t.ClaimAgv(Result.DeviceID);
                                t.ClaimTaskState(HBAgvTaskState.CancelAndRelease);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                        }
                    }

                    //任务不存在，则添加。
                    else
                    {
                        Result = Result = HttpClientHelper.Post<TaskResponse>(HBUrl + DeliveryRequestUrl, new HBTaskParam() {
                            agvID = t.TargetAgvId,
                            requestCode = Guid.NewGuid().ToString(),
                            startCode = "",
                            TaskId = t.TaskIdInHB,
                            isWait = !t.NeedReleaseAgv, 
                            targetCode = t.TargetNodeName
                        });

                        if (!Result.IsSuccess())
                        {
                            logger.LogWarning("添加任务失败:" + Result.describe);
                        }
                    }
                }

                else if (t.TaskName == HBAgvTask.Move)
                {
                    //1.通过HB的api，查询状态；
                    var Result = HttpClientHelper.Post<TaskResponse>(HBUrl + GetTaskStatusUrl, new { TaskID = t.TaskIdInHB }); 
                    //2.任务存在，则根据查询到的状态更新PDS中的记录
                    if (Result.IsSuccess())
                    {
                        switch (Result.taskStatusCode)
                        {
                            case HBTaskStatusType.Waitting:
                            case HBTaskStatusType.Working:
                                break;

                            case HBTaskStatusType.Finished:
                                t.ClaimTaskState(HBAgvTaskState.FinishedAndWaitting);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                            case HBTaskStatusType.Cancel:
                                t.ClaimTaskState(HBAgvTaskState.CancelAndRelease);
                                await this.hBAgvTasksRepos.UpdateAsync(t);
                                break;
                        }
                    }

                    //任务不存在，则添加。
                    else
                    {
                        Result = HttpClientHelper.Post<TaskResponse>(HBUrl + MoveUrl, new HBTaskParam()
                        {
                            agvID = t.TargetAgvId,
                            requestCode = Guid.NewGuid().ToString(),
                            startCode = "",
                            TaskId = t.TaskIdInHB,
                            isWait = !t.NeedReleaseAgv,
                            targetCode = t.TargetNodeName,
                        });

                        if(!Result.IsSuccess())
                        {
                            logger.LogWarning("添加任务失败:" + Result.describe);
                        }
                    }
                }
                else
                {
                    //error
                }
            }

        }
    }



    #region 客户端的dto

    public class HBTaskParam
    {
        /// <summary>
        /// 请求编号
        /// </summary>
        public string requestCode { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 任务Id
        /// </summary>
        public string TaskId { get; set; }
        /// <summary>
        /// agvID,没有指定时，系统自动分配
        /// </summary>
        public string agvID { get; set; }

        /// <summary>
        /// 取货点，为空时，直接去目标点
        /// </summary>
        public string startCode { get; set; }

        /// <summary>
        /// 运行的目标点
        /// </summary>
        public string targetCode { get; set; }

        /// <summary>
        /// 任务完成后，是否需要保持agv
        /// </summary>
        public bool isWait { get; set; }

    }


    /// <summary>
    /// 添加任务的返回数据
    /// </summary>
    public class TaskResponse
    {

        /// <summary>
        /// 唯一请求标示ID
        /// </summary>
        public string requestCode;

        /// <summary>
        /// 反馈操作状态， 0表示成功，其他值表示失败
        /// </summary>
        public int responseCode = -1;

        /// <summary>
        /// 任务状态, 
        /// Waitting=0x01<<0,    //等待执行
        /// Working=0x01<<1,    //正在执行
        /// Finished=0x01<<2,   //已经完成
        ///Cancel=0x01<<3      //取消
        /// </summary>
        public HBTaskStatusType taskStatusCode = HBTaskStatusType.Waitting;

        /// <summary>
        /// 若任务添加成功的任务ID
        /// </summary>
        public string taskID;

        /// <summary>
        /// 执行任务的agvID
        /// </summary>
        public string DeviceID = null;

        public string describe = "";

        public bool IsSuccess()
        {
            return responseCode == 0;
        }


    }

    public enum HBTaskStatusType
    {
        //任务状态, 
        Waitting = 0x01 << 0,    //等待执行
        Working = 0x01 << 1,    //正在执行
        Finished = 0x01 << 2,   //已经完成
        Cancel = 0x01 << 3      //取消
    }


    #endregion
}
