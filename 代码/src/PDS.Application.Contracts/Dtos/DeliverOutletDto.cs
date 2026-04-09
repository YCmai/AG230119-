using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace PDS.Domain.Entitys
{
    public class DeliverOutletDto : EntityDto<string>
    {
        /// <summary>
        /// 投递口状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 投递口类型
        /// </summary>
        public string DeliverType { get; set; }
        /// <summary>
        /// 绑定的上层地图节点（这个用于投递agv任务的控制）
        /// </summary>
        public string UppperMapNodeName { get; set; }
        /// <summary>
        /// 绑定的下层地图节点（这个用于笼车的关联，如果笼车当前停靠点与这个点一致，则可以关联）
        /// </summary>
        public string DownMapMapNodeName { get; set; }
        /// <summary>
        /// 绑定的笼车Id；有笼车Id，才能进行投递；
        /// </summary>
        public string CageCarId { get; set; }
        /// <summary>
        /// 已投数量
        /// </summary>
        public int PackageCount { get; set; }
        /// <summary>
        /// 在投数量
        /// </summary>
        public int OnDeliverCount { get; set; }
        /// <summary>
        /// 投递口分类
        /// </summary>
        public string PackageSortid { get; set; }
        /// <summary>
        /// 最大投递数量
        /// </summary>
        public int MaxPackageCount { get; set; }
    }





}
