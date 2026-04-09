using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace WMS.StorageModule.Domain
{
    //实体注意事项：
    //聚合根继承AggregateRoot或者使用BasicAggregateRoot，提示继承Entity<T> ，值类型、或者组合key的继承Entity。
    //实体必须考虑set的作用范围，思考自身属性的修改规则并提供对应方法。（领域方法）

    //AggregateRoot 继承了IHasExtraProperties，IHasConcurrencyStamp ，如果不想用，使用BasicAggregateRoot；



    /// <summary>
    /// Id 是string，表示储位的编号或者唯一识别码。
    /// </summary>
    public class Storage : AggregateRoot<string>
    {
        protected Storage()
        {
            Locks = new List<StorageLock>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StorageId">储位Id</param>
        /// <param name="parentId">父储位</param>
        /// <param name="OUCode"></param>
        /// <param name="mapNodeName"></param>
        /// <param name="creatorId"></param>
        /// <param name="storageCategory"></param>
        /// <param name="name">储位名称</param>
        /// <param name="sizeMes"></param>
        /// <param name="isContainer"></param>
        /// <param name="appData"></param>
        internal Storage(string StorageId,  string parentId, string OUCode, string mapNodeName, Guid creatorId, string storageCategory,
            string name, string sizeMes,  string appData1,string appData2, int startHeight,
            string wareHouseId,string wareHouseAreaCode) : base(StorageId)
        {
            this.ParentId = parentId;
            this.OUCode = OUCode;
            this.MapNodeName = mapNodeName;
            this.CreatorId = creatorId;
            this.Category = storageCategory;
            this.SizeMes = sizeMes;
            this.AppData1 = appData1;
            this.AppData2 = appData2;
            this.CreationTime = DateTime.Now;
            this.WareHouseId = wareHouseId;
            this.WareHouseIdAreaCode = wareHouseAreaCode;
            Name = name;
            Locks = new List<StorageLock>();
            StartHeight = startHeight;
        }

        /// <summary>
        /// 留用数据；根据具体的应用来使用。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string AppData1 { get; set; }
        /// <summary>
        /// 留用数据；根据具体的应用来使用
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string AppData2 { get; set; }
        /// <summary>
        /// 绑定的agv地图节点，可空。
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string MapNodeName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; protected set; }
        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid CreatorId { get; protected set; }
        /// <summary>
        ///  储位类别
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Category { get; set; }
        /// <summary>
        /// 储位名称
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; }
        /// <summary>
        /// 尺寸信息（可空，具体与应用有关，长宽高使用字母x隔开）
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string SizeMes { get; set; }
        /// <summary>
        /// 储位的起始高度（mm，可空）（常用于控制叉臂叉取货物）
        /// </summary>
        public int StartHeight { get; set; }
        /// <summary>
        /// 所属仓库Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseId { get; private set; }
        /// <summary>
        /// 所属区域Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string WareHouseIdAreaCode { get; private set; }
        /// <summary>
        /// 当前节点的物料数量
        /// </summary>
        public int CurrentNodeMaterialCount { get; protected set; }

        #region 组织架构
        /// <summary>
        /// 父储位的Id
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string ParentId { get; internal set; }
        /// <summary>
        /// 组织编码,每层编码规则为6位数（“000001~999999”）， 子节点的OuCode = “父节点OUCode” + “新的编码”，同一层编码不重复。 最大是16层，工111个字符。 todo，总有一点会爆。
        /// </summary>
        [Column(TypeName = "nvarchar(128)")]
        public string OUCode { get; internal set; }

        #endregion

        /// <summary>
        /// 储位锁。储位锁本身不属于储位的属性，它的具体的功能取决于上层系统。一般用于表示关联了任务。
        /// </summary>
        public List<StorageLock> Locks { get; private set; }

        public void SetMaterialCount(int value)
        {
            this.CurrentNodeMaterialCount = value;
            if (value < 0) throw new Exception("物料数量不能小于0");
        }

        public void AddTaskLock(string lockerId, StorageLockType taskLockType)
        {
            Locks.Add(new StorageLock(this.Id, lockerId, taskLockType));
        }
        public void RemoveTaskLock(string lockerId)
        {
            var l = this.Locks.FirstOrDefault(p => p.LockerId == lockerId);
            if (l == null) throw new Exception("对象不存在");
            this.Locks.Remove(l);
        }

        public bool TryRemoveLock(string lockerId)
        {
            var l = this.Locks.FirstOrDefault(p => p.LockerId == lockerId);
            if (l == null) return false;
            return this.Locks.Remove(l);
        }
    }


    public class StorageLock : Entity
    {
        protected StorageLock() { }

        internal StorageLock(string storageId,string lockerId,StorageLockType taskLockType)
        {
            this.StorageId = storageId;
            this.LockerId = lockerId;
            this.LockType = taskLockType;
        }
        [Column(TypeName = "nvarchar(64)")]
        public string StorageId { get; private set; }

        [Column(TypeName = "nvarchar(64)")]
        public string LockerId { get; private set; }
        public StorageLockType LockType { get; private set; }

        public override object[] GetKeys()
        {
            return new object[] { StorageId, LockerId };
        }
    }

    public enum StorageLockType:int
    {
        /// <summary>
        /// 迁入锁，表示当前储位绑定了入库相关的任务
        /// </summary>
        MaterialIn = 0,
        /// <summary>
        /// 迁出锁，表示当前储位绑定了出库相关的任务
        /// </summary>
        MaterialOut = 1
    }

    public static class StorageOUConsts
    {
        /// <summary>
        /// Maximum length of the DisplayName property.
        /// </summary>
        public static int MaxDisplayNameLength { get; set; } = 128;

        /// <summary>
        /// Maximum depth of an OU hierarchy.
        /// </summary>
        public const int MaxDepth = 16;

        /// <summary>
        /// Length of a code unit between dots.
        /// </summary>
        public const int CodeUnitLength = 6;

        /// <summary>
        /// Maximum length of the Code property.
        /// </summary>
        public const int MaxCodeLength = MaxDepth * (CodeUnitLength + 1) - 1;
    }
}
