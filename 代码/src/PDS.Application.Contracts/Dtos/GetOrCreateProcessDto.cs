using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace PDS.Application.Contracts.Dtos
{
    public class GetOrCreateProcessDto 
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 还书机是否需要把书投出去（给agv）
        /// </summary>
        public bool PutBook { get; set; }
    }

   



}
