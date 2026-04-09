using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class ClearPackagesInput : EntityDto
    {
        /// <summary>
        /// 数据帧Id
        /// </summary>
        public string FrameId { get; set; }
        /// <summary>
        /// 投递口Id
        /// </summary>
        public string DeliverOutetId { get; set; }
        
    }


    
}
