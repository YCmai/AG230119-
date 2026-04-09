using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PDS.Domain.Entitys;

namespace PDS.Domain.Interface
{
    /// <summary>
    /// 投递口管理
    /// </summary>
    public interface IDeliverOutletManager
    {
        ///// <summary>
        ///// 锁定（当前没有任务才可以锁定）
        ///// </summary>
        //void LockDeliverOutlet(DeliverOutlet deliverOutlet);
        ///// <summary>
        ///// 释放
        ///// </summary>
        ///// <param name="deliverOutlet"></param>
        //void ReleaseDeliverOutlet(DeliverOutlet deliverOutlet);
        ///// <summary>
        ///// 锁定中
        ///// </summary>
        ///// <param name="deliverOutlet"></param>
        //void LockingDeliverOutlet(DeliverOutlet deliverOutlet);
        ///// <summary>
        ///// 给包裹划分分类
        ///// </summary>
        ///// <param name="package"></param>
        //void AllocPackageSort(Package package);
        ///// <summary>
        ///// 给笼车划分分类
        ///// </summary>
        ///// <param name="cageCar"></param>
        //void AllocCageCarSort(CageCar cageCar);
        ///// <summary>
        ///// 给包裹分配具体的投递口
        ///// </summary>
        ///// <param name="package"></param>
        //void AllocPackageDeliverOutlet(Package package);
        ///// <summary>
        ///// 投递口和笼车绑定
        ///// </summary>
        ///// <param name="deliverTask"></param>
        ///// <param name="cageCar"></param>
        //void BindingCageCar(DeliverProcessTask deliverTask, CageCar cageCar);


        ////给包裹确定投递口（先要确定分类）
        //bool TryFindDeliverOut(Package package, out CageCar cageCar,out DeliverOutlet deliverOutlet);

        ////包裹必须是没有绑定任务的；
        ////投递口是有绑定笼车的；
        ////投递口的状态是没有被锁定；也不是锁定中；也不是禁用；
        ////笼车的状态是DeliverAvailable
        ////笼车的分类与包裹的分类是一致的。


        ///// <summary>
        ///// 给包括确定分类；
        ///// </summary>
        ///// <param name="package"></param>
        ///// <returns></returns>
        //bool AllocPackSort(Package package);


        void Binding(DeliverOutlet deliverOutlet, CageCar cageCar);

        void ReleaseBinding(DeliverOutlet deliverOutlet, CageCar cageCar);



    }
}
