using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class GetStationsRequestDto
    {
        /// <summary>
        /// 请求ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public OperationStationState? State { get; set; }
    }
}
