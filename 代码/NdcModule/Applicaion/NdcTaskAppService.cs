using AciModule.Domain.Entitys;
//using AciModule.Domain.Queue;
using AciModule.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;

namespace AciModule.Applicaion
{
    [Route("api/NdcTask")]
    [DisableAuditing]
    public class NdcTaskAppService: ApplicationService
    {
        private readonly IRepository<NdcTask_Moves> _ndcTaskmove;
        static object LockObjct = new object();
        public NdcTaskAppService(IRepository<NdcTask_Moves> ndcTaskmove)
        {
            _ndcTaskmove = ndcTaskmove;
        }
        /// <summary>
        /// 返回0或者负数，需要重新获取
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetNdcTaskId")]
        public async Task<int> GetNdcTaskId()
        {
            //try
            //{
            //    lock (LockObjct)
            //    {
            //        //生成一个id，从数据库中查询正在执行的任务有没有存在该同样的id，如果存在则重新生成 
            //        var ndcTasks = _ndcTaskmove.GetQueryableAsync().Result
            //             .Select(i => new
            //             {
            //                 i.TaskStatus,
            //                 i.NdcTaskId
            //             })
            //            .Where(i => i.TaskStatus != TaskStatuEnum.TaskFinish)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.CanceledWashFinish)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.OrderAgvFinish)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.Canceled)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.InvalidUp)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.RedirectRequest)
            //            .Where(i => i.TaskStatus != TaskStatuEnum.InvalidDown)
            //            .OrderBy(i => i.NdcTaskId)
            //            .ToList();
            //        //创建一个数组长度
            //        int[] ndcId = new int[ndcTasks.Count];
            //        for (int i = 0; i < ndcTasks.Count; i++)
            //        {
            //            ndcId[i] = ndcTasks[i].NdcTaskId;
            //        }
            //        if (ndcId.Length > 0) return GetRandom.GetId(ndcId);
            //        if (ndcId.Length <= 0) return GetRandom.GenerateRandomSeed();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            //return 0;
            try
            {
                var ndcTasks = await _ndcTaskmove.GetQueryableAsync();
                var activeTasks = ndcTasks
                    .Where(i => i.TaskStatus != TaskStatuEnum.TaskFinish)
                    .Where(i => i.TaskStatus != TaskStatuEnum.CanceledWashFinish)
                    .Where(i => i.TaskStatus != TaskStatuEnum.OrderAgvFinish)
                    .Where(i => i.TaskStatus != TaskStatuEnum.Canceled)
                    .Where(i => i.TaskStatus != TaskStatuEnum.InvalidUp)
                    .Where(i => i.TaskStatus != TaskStatuEnum.RedirectRequest)
                    .Where(i => i.TaskStatus != TaskStatuEnum.InvalidDown)
                    .Select(i => i.NdcTaskId)
                    .OrderBy(id => id)
                    .ToList();

                if (activeTasks.Any())
                {
                    return GetRandom.GetId(activeTasks.ToArray());
                }
                else
                {
                    return GetRandom.GenerateRandomSeed();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }
    }
}
