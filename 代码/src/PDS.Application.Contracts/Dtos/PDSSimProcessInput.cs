using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{




    /// <summary>
    /// Package非空，表示有书。此时放置任务还没有下发，所以CurrentPutTaskId
    /// </summary>
    [Serializable]
    public class PDSSimProcessInput : EntityDto
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 还书机Id
        /// </summary>
        public string OperationStationId { get; set; }
        /// <summary>
        /// 包裹编号;如果是空，表示没有包裹。包裹编号是唯一的。
        /// </summary>
        public string PackageCode { get; set; }
        /// <summary>
        /// 包裹的扩展信息
        /// </summary>
        public string PackageExtInfo { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public BookState State { get; set; }
        /// <summary>
        /// 如果指定了分类，就按分类。没有指定分类，按内部逻辑进行分类。
        /// </summary>
        public string PackageSortDecribtion { get; set; }
    }

    public enum BookState
    {
        /// <summary>
        /// 当前没有书本。（也可以能是书本已经投完）
        /// </summary>
        NoBook,
        /// <summary>
        /// 当前有未处理书本。此时PackageCode有效，表示还书机在等待PDS下发投书任务。
        /// </summary>
        HasUnhadleBook,
        /// <summary>
        /// 当前正在执行投书任务。
        /// </summary>
        PuttingBook
    }


}
