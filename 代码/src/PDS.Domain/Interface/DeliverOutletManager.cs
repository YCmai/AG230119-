using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PDS.Domain.Entitys;
using Volo.Abp.DependencyInjection;

namespace PDS.Domain.Interface
{
    /// <summary>
    /// 投递口管理
    /// </summary>
    public class DeliverOutletManager:IDeliverOutletManager,ITransientDependency
    {
        /// <summary>
        /// 投递口和笼车的相互绑定。
        /// </summary>
        /// <param name="deliverOutlet"></param>
        /// <param name="cageCar"></param>
        public void Binding(DeliverOutlet deliverOutlet, CageCar cageCar)
        {
            if (deliverOutlet.PackageSortId.IsNullOrWhiteSpace()) throw new Exception("投递口类型未指定，无法绑定");
            deliverOutlet.BindingCageCar(cageCar);
            cageCar.BingdingDeliverOutlet(deliverOutlet.Id, deliverOutlet.PackageSortId);
        }


        public void ReleaseBinding(DeliverOutlet deliverOutlet, CageCar cageCar)
        {
            deliverOutlet.ReleaseBingding(cageCar.Id);
            cageCar.ReleaseBindingDeliverOutlet(deliverOutlet.Id);
        }
    }
}
