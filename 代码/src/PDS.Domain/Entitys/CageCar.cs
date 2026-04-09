using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    
    public class CageCar: BasicAggregateRoot<string>
    {

        protected CageCar() { }

        public CageCar(string Id, string currentStorageId)
        {
            this.Id = Id;
            this.CurrentStorageId = currentStorageId;
            this.State = CageCarState.PACKAGE_AVAILABLE;
        }
        /// <summary>
        /// 包裹的分类（表示这个笼车用于处理什么分类的包裹，当与投递口绑定时，指定）
        /// </summary>
        public string PackageSortId { get; protected set; }
        /// <summary>
        /// 已关联的投递口
        /// </summary>
        public string DeliverOutletId { get; protected set; }
        /// <summary>
        /// 停靠的储位Id，如果为空，则表示没有停靠的储位。
        /// </summary>
        public string CurrentStorageId { get; protected set; }
        /// <summary>
        ///  笼车中包含的包裹的对应的流程（这些包裹都已经投递成功）。
        /// </summary>
        public virtual List<CageCarLinkPackage> Packages { get; protected set; } = new List<CageCarLinkPackage>();

        public CageCarState State { get; protected set; }

        public void AddPackageLink(string PackageId)
        {
            if (Packages.FirstOrDefault(p => p.PackageId.Equals(PackageId)) != null) throw new Exception("已经有相同的包裹信息");
            Packages.Add(new CageCarLinkPackage(this.Id, PackageId));
        }


        /// <summary>
        /// 添加绑定。   
        /// </summary>
        /// <param name="DeliverOutletId"></param>
        public void BingdingDeliverOutlet(string DeliverOutletId, string DeliverOutletPackageSort)
        {

            if (!this.DeliverOutletId.IsNullOrWhiteSpace()) throw new Exception("笼车已经被绑定，无法绑定");
            this.DeliverOutletId = DeliverOutletId;
            this.PackageSortId = DeliverOutletPackageSort;
        }

        public void ReleaseBindingDeliverOutlet(string DeliverOutletId)
        {
            if(!DeliverOutletId.Equals(this.DeliverOutletId)) throw new Exception("笼车没有绑定此投递口，无法取消绑定");
            this.DeliverOutletId = "";
        }

        /// <summary>
        /// 清空包裹，并设置重置笼车的物料分类
        /// </summary>
        public void ClearPackageAndResetSort()
        {
            if (!this.DeliverOutletId.IsNullOrWhiteSpace()) throw new Exception("笼车已与投递口绑定，无法清空笼车。");
            this.Packages.Clear();
            this.State = CageCarState.PACKAGE_AVAILABLE;
            this.PackageSortId = "";
        }

        /// <summary>
        /// 声明笼车当前储位
        /// </summary>
        /// <param name="StrogeId"></param>
        public void ClaimCurrentStorage(string StorageId)
        {
            this.CurrentStorageId = StorageId;
        }
        /// <summary>
        /// 声明笼车的状态
        /// </summary>
        /// <param name="cageCarContainerState"></param>
        public void ClaimState(CageCarState cageCarContainerState)
        {
            this.State = cageCarContainerState;
        }
    }

    public class CageCarLinkPackage : Entity
    {
        protected CageCarLinkPackage() { }

        public CageCarLinkPackage(string CageCarID, string DeliverTaskId)
        {
            this.CageCarId = CageCarID;
            this.PackageId = DeliverTaskId;
        }
        /// <summary>
        /// 笼车Id
        /// </summary>
        public virtual string CageCarId { get; protected set; }
        /// <summary>
        /// 投递工作流
        /// </summary>
        public string PackageId { get; protected set; }

        public override object[] GetKeys()
        {
            return new object[] { CageCarId, PackageId };
        }
    }

    public enum CageCarState
    {
        /// <summary>
        /// 可用，表示可以接收新包裹。
        /// </summary>
        PACKAGE_AVAILABLE,
        /// <summary>
        /// 关闭，表示不应该再接收新包裹。需要清空。
        /// </summary>
        PACKAGE_CLOSE,
    }
}
