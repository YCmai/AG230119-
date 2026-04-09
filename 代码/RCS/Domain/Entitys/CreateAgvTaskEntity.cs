using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Domain.Entitys
{
    public class CreateAgvTaskEntity
    {
        public string TaskCode { get; set; }
        /// <summary>
        /// 0出库，1入库，2出库到产线
        /// </summary>
        public int TaskType { get; set; }
        public int PickupPoint { get; set; }
        public int PickupHeight { get; set; }
        public int UnloadPoint { get; set; }
        public int UnloadHeight { get; set; }
        public int TaskStatus { get; set; }
        public int Priority { get; set; }=0;
       
    }
}
