using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    /// <summary>
    /// 笼车控制点（类似于wms的储位）
    /// </summary>
    public class CageCarStorage: BasicAggregateRoot<string>
    {
        protected CageCarStorage() { }
       
        public CageCarStorage(string mapNodeName,CageCarCageCarStorageType cageCarCageCarStorageType)
        {
            Id = Guid.NewGuid().ToString();
            this.MapNodeName = mapNodeName;
            this.StorageType = cageCarCageCarStorageType;
        }
        /// <summary>
        /// 关联的地图节点
        /// </summary>
        public string MapNodeName { get; protected set; }
        /// <summary>
        /// 控制点类型（类似于wms的储位类型）
        /// </summary>
        public CageCarCageCarStorageType StorageType { get; protected set; }
    }


    public enum CageCarCageCarStorageType : int
    {
        /// <summary>
        /// 笼车投递停靠点
        /// </summary>
        DELIVER_POINT = 0,
        /// <summary>
        /// 空车缓存点
        /// </summary>
        EMPTY_CAR_BUFF = 1,
        /// <summary>
        /// 满车缓存点
        /// </summary>
        FULL_CAR_BUFF = 2,
    }
}
