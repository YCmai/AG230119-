using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{

    [Serializable]
    public class HBCommInfoDto : EntityDto
    {
        /// <summary>
        /// 笼车Id
        /// </summary>
        public CommStatus commStatus { get; set; }
        /// <summary>
        /// 上一次通讯时间（只统计上线通讯）
        /// </summary>
        public DateTime LastCommTime { get; set; }


    }

    public enum CommStatus
    {
        OnLine,
        OutLine,
        TimeOut,
    }
}
