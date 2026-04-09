using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class CageCarUpdateInput : EntityDto<string>
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
