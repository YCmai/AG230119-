namespace WarehouseManagementSystem.Models
{
    public class Rcs_Plcs
    {
        //public string DBAddress { get; set; }

        //public string Type { get; set; }

        //public string IP { get; set; }

        //public string AddressRemark { get; set; }

        //public string DBType { get; set; }


        //public string Value { get; set; }

        //public DateTime? UpdateTime { get; set; }


        /// <summary>
        /// Io名字
        /// </summary>
        public string IoName { get; set; }

        /// <summary>
        /// IO地址
        /// </summary>
        public string IoAddress { get; set; }

        /// <summary>
        /// IO状态
        /// </summary>
        public bool IoStatus { get; set; }


        /// <summary>
        /// IO地址备注
        /// </summary>
        public string IoAddressRemark { get; set; }


        /// <summary>
        /// IO IP地址
        /// </summary>
        public string IoIp { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
