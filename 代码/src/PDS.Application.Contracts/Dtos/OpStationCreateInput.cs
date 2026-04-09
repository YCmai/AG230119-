using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class OpStationCreateInput:EntityDto
    {
        public string FrameId { get; set; }
        public string OperationStationId { get; set; }
        public string BindingMapName { get; set; }
        public int MaxAgvCount { get; set; }
        public string loadRobotId { get; set; }
    }
}
