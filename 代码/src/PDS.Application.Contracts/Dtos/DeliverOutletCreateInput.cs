using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class DeliverOutletCreateInput : EntityDto
    {
        /// <summary>
        /// 数据帧Id
        /// </summary>
        public string FrameId { get; set; }
        /// <summary>
        /// 投递口Id
        /// </summary>
        public string DeliverOutletId { get; set; }
        /// <summary>
        /// 投递口类型
        /// </summary>
        public DeliverOutletType DeliverOutletType { get; set; }
        /// <summary>
        /// 投递口中心对应的上层地图节点
        /// </summary>
        public string UppperMapNodeName { get; set; }
        /// <summary>
        /// 投递口中心对应的下层地图节点
        /// </summary>
        public string DownMapMapNodeName { get; set; }
        /// <summary>
        /// 投递口分类,如果是空，则无所谓
        /// </summary>
        public string PackageSortid { get; set; }
    }


    
}
