using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{

    [Serializable]
    public class CageCarCreateInput : EntityDto
    {
        /// <summary>
        /// 数据帧Id
        /// </summary>
        public string FrameId { get; set; }
        /// <summary>
        /// 笼车Id
        /// </summary>
        public string CageCaId { get; set; }
        /// <summary>
        /// 储位Id
        /// </summary>
        public string StorageId { get; set; }


    }
}
