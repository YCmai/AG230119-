using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using HBTaskModule.Domain.Shared;
using HBTaskModule.Domain;
using Volo.Abp.Application.Services;

namespace WMS.WaveOrderModule.Application
{
    [Route("api/HBTaskTest")]
    public class HBTaskTestAppservice : ApplicationService
    {
        private readonly IRepository<HBTask_Allocation, Guid> _hBTask_AllocationRepos;
        private readonly IRepository<HBTask_Load, Guid> _hBTask_LoadRepos;
        private readonly IRepository<HBTask_UnLoad, Guid> _hBTask_UnLoadRepos;
        private readonly IRepository<HBTask_Move, Guid> _hBTask_MoveRepos;
        private readonly IRepository<HBTask_Release, Guid> _hBTask_ReleaseRepos;

        public HBTaskTestAppservice(
            IRepository<HBTask_Allocation, Guid> hBTask_AllocationRepos,
            IRepository<HBTask_Load,Guid> hBTask_LoadRepos,
            IRepository<HBTask_UnLoad,Guid> hBTask_UnLoadRepos,
            IRepository<HBTask_Move,Guid> hBTask_MoveRepos,
            IRepository<HBTask_Release,Guid> hBTask_ReleaseRepos
            )
        {
            _hBTask_AllocationRepos = hBTask_AllocationRepos;
            _hBTask_LoadRepos = hBTask_LoadRepos;
            _hBTask_UnLoadRepos = hBTask_UnLoadRepos;
            _hBTask_MoveRepos = hBTask_MoveRepos;
            _hBTask_ReleaseRepos = hBTask_ReleaseRepos;
        }

        /// <summary>
        /// 这个用来手动设置HB任务已经完成，模拟任务的完成。
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost("UpdateHBTaskFinish")]
        public async Task CreateAllocationAsync(Guid Id)
        {
            //var entity = new HBTask_Allocation(Guid.NewGuid(), Guid.NewGuid(), this.GetType().FullName, "", EAgvType.AnyType, EAllcateType.AllocateWhenCatch, "N10001");
            //entity = await this._hBTask_Allocations.InsertAsync(entity);

            var entity_allcation = await this._hBTask_AllocationRepos.FindAsync(Id);
            if (entity_allcation != null)
            {
                entity_allcation.ActualAgvId = "Agv01";
                entity_allcation.SetStatus(EAllocationStatus.Success);
                await this._hBTask_AllocationRepos.UpdateAsync(entity_allcation);
                return;
            }


            var entity_load = await this._hBTask_LoadRepos.FindAsync(Id);
            if (entity_load != null)
            {
                entity_load.SetStatus(ELoadStatus.Success);
                await this._hBTask_LoadRepos.UpdateAsync(entity_load);
                return;
            }


            var entity_Unload = await this._hBTask_UnLoadRepos.FindAsync(Id);
            if (entity_Unload != null)
            {
                entity_Unload.SetStatus(EUnloadStatus.Success);
                await this._hBTask_UnLoadRepos.UpdateAsync(entity_Unload);
                return;
            }

            var entity_Move = await this._hBTask_MoveRepos.FindAsync(Id);
            if(entity_Move!=null)
            {
                entity_Move.SetStatus(EMoveStatus.Success);
                await this._hBTask_MoveRepos.UpdateAsync(entity_Move);
                return;
            }

            var entity_Relase = await this._hBTask_ReleaseRepos.FindAsync(Id);
            if(_hBTask_ReleaseRepos!=null)
            {
                entity_Relase.SetStatus(EReleaseStatus.Success);
                await this._hBTask_ReleaseRepos.UpdateAsync(entity_Relase);
                return;
            }
        }



    }
}

