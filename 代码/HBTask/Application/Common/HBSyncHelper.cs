using HBTaskModule.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskBaseModule.Domain.Shared;

namespace HBTaskModule.Application
{
    internal static class HBSyncHelper
    {
        /// <summary>
        /// 检查是否已经向HB添加了指定任务，如果没有则添加。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="HBTask"></param>
        public static async Task<SyncState> CheckOrAddCheckAsycn(TaskBaseAggregateRoot HBTask)
        {
            //todo
            if(HBTask is HBTask_Allocation)
            {

            }
            else if(HBTask is HBTask_Load)
            {

            }
            else if(HBTask is HBTask_UnLoad)
            {

            }
            else if(HBTask is HBTask_Move)
            {

            }
            else if(HBTask is HBTask_Release)
            {

            }
            
            
            return SyncState.Fault;
        }
    }

    internal enum SyncState
    {
        /// <summary>
        /// 同步成功，确认HB系统已经接收到本次任务或者是之前已经收到
        /// </summary>
        Success,
        /// <summary>
        /// 同步失败，确认HB系统没有接收到本次任务的发送
        /// </summary>
        Fault,
        /// <summary>
        /// 未知。不确定的问题，不知道是收到还是没有收到。
        /// </summary>
        Unknow,
    }
}
