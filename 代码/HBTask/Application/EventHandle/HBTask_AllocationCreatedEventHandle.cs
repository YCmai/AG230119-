using HBTaskModule.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace HBTaskModule.Application
{

    /// <summary>
    /// 这里是添加了任务以后，马上尝试发送数据给HB。这里没有也是可以的，会定时触发。
    /// </summary>
    public class HBTask_AllocationCreatedEventHandle : ILocalEventHandler<EntityCreatedEventData<HBTask_Allocation>>, ISingletonDependency
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public HBTask_AllocationCreatedEventHandle(
            IServiceScopeFactory serviceScopeFactory,
            IRepository<HBTask_Allocation, Guid> repos,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _unitOfWorkManager = unitOfWorkManager;
        }


        public async Task HandleEventAsync(EntityCreatedEventData<HBTask_Allocation> eventData)
        {
            var currentUow = _unitOfWorkManager.Current;
            if (currentUow == null || currentUow.IsDisposed || currentUow.IsCompleted || currentUow.IsReserved) return;

            currentUow.OnCompleted(async () =>  //todo,apb5.2.2取消ed事件以后（ing改为名ed，原来的ed取消），于是改为这个形式,这里实际就是注册了一个事件，等事务提交后触发这里的函数。
            {
                if (_unitOfWorkManager.Current != null) return;  //这里预计是null的，为了谨慎起见，不是null就直接返回吧。
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _hBTask_AllocationRepos = scope.ServiceProvider.GetRequiredService<IRepository<HBTask_Allocation, Guid>>();
                    var allocationTask = await _hBTask_AllocationRepos.GetAsync(eventData.Entity.Id);

                    //如果没有同步就同步。
                    if (allocationTask.AllcateStatus == EAllocationStatus.NotSync)
                    {
                        allocationTask.SetStatus(EAllocationStatus.Syncing);
                        allocationTask = await _hBTask_AllocationRepos.UpdateAsync(allocationTask);

                        //这有一个关键点：调用下面api必须是数据库已经修改为Syncing，否则可能会出现问题。注意，别被事务回滚了，确定没有事务。刚好这里是的当前uow是null，所以上面的更改会马上落实到数据库中。


                        //如果同步成功（添加成功或者已经有相同的id的对象），修改同步状态为Synced，
                        var re = await HBSyncHelper.CheckOrAddCheckAsycn(allocationTask);
                        if (re == SyncState.Success)
                        {
                            allocationTask.SetStatus(EAllocationStatus.Excuting);
                            await _hBTask_AllocationRepos.UpdateAsync(allocationTask);
                        }
                        else if (re == SyncState.Fault)
                        {
                            //原来的状态就是NotSync，本次通讯确定是对方没有收到，那么状态应该恢复到NotSync
                            allocationTask.SetStatus(EAllocationStatus.NotSync);
                            await _hBTask_AllocationRepos.UpdateAsync(allocationTask);
                        }
                        else //未知，就不用更新，保持原来的Syncing状态就可以了。
                        {
                            //allocationTask.SyncStatus = ESyncState.Syncing;
                            //await _hBTask_AllocationRepos.UpdateAsync(allocationTask);
                        }
                    }
                }
            });

            await Task.CompletedTask;

        }
    }

}
