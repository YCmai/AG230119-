using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TaskBaseModule.Domain.Shared
{
    public interface IRepositoryTaskBase { }

    public interface IRepositoryTaskBase<TTask>:IRepository<TTask,Guid>,IRepositoryTaskBase where TTask:TaskBaseAggregateRoot
    {
        Type TaskType { get; }
    }


}
