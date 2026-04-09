using PDS.Application.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class GetOperaionStationResultDto
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        public List<OperationStationDto> Items { get; set; }
    }
}
