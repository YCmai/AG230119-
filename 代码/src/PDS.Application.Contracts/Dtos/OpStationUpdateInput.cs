using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS.Dtos
{
    public class OpStationUpdateInput:EntityDto
    {
        public string FrameId { get; set; }
        public string OperationStationId { get; set; }
        public int MaxAgvCount { get; set; }
    }
}
