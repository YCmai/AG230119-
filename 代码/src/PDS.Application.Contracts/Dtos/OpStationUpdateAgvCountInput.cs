using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class OpStationUpdateAgvCountInput : EntityDto
    {
        public string FrameId { get; set; }
        public string OperationStationId { get; set; }
        public int MaxAgvCount { get; set; }
    }
}
