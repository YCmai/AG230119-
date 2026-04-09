using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PDS
{
    public class PackageProcessDayCount
    {
        [JsonIgnore]
        public DateTime Date { get; set; }

        public int PackageCount { get; set; }

        public string Day { get { return Date.ToString("M"); } }
    }
}
