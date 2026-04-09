using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
   
    [Serializable]
    public class OpStationTodayPackageStatisticsInput
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 操作点Id
        /// </summary>
        public string OperationId { get; set; }
    }
}
