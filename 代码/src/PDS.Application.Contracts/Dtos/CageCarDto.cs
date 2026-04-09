using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace PDS.Application.Contracts.Dtos
{
    public class CageCarDto : EntityDto<string>
    {
        /// <summary>
        /// 包裹的分类
        /// </summary>
        public string PackageSortDescribtion { get; set; }
        /// <summary>
        /// 已关联的投递口
        /// </summary>
        public string DeliverOutletId { get; set; }
        /// <summary>
        /// 停靠的储位Id，如果为空，则表示没有停靠的储位。
        /// </summary>
        public string CurrentStorageId { get; set; }
        /// <summary>
        /// 类型(CurrentStorageId有值，此属性才有效）
        /// </summary>
        public CageCarCageCarStorageTypeDto CurrentStorageType { get; set; }
        /// <summary>
        ///  笼车中包含的包裹的对应的流程（这些包裹都已经投递成功）。
        /// </summary>
        public int PakageCount { get; set; }
        /// <summary>
        /// 笼车状态
        /// </summary>
        public CageCarStateDto State { get; set; }

        public string StorageNodeName { get; set; }
    }

   
    public enum CageCarStateDto
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

    public enum CageCarCageCarStorageTypeDto : int
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
