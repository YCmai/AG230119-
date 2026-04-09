using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace PDS.Application.Interface
{
    /// <summary>
    /// 投递点分配器
    /// </summary>
    public interface IDeliverOutletAllocator
    {
        /// <summary>
        /// 从指定的投递口中，为包裹选择配合适的投递口。
        /// </summary>
        /// <returns>
        /// 适合于分配的投递口对象
        /// </returns>
        DeliverOutlet AllocateDeliverOutlet(PackageSort packageSort,List<DeliverOutlet> deliverOutlets);
    }


    public class DeliverOutletAllocator : IDeliverOutletAllocator, ITransientDependency
    {
        //todo,这里是随便写的，认为投递到第一个就ok了。
        public DeliverOutlet AllocateDeliverOutlet(PackageSort packageSort, List<DeliverOutlet> deliverOutlets)
        {
            return deliverOutlets.FirstOrDefault(p=>p.State ==  DeliverOutletState.DELIVER_AVAILABLE && p.PackageSortId == packageSort.Id);
        }
    }

}
