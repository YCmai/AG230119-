using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PDS
{
    public class PackageProcessHourCount
    {
        [JsonIgnore]
        public DateTime Date { get; set; }

        public int PackageCount { get; set; }
        [JsonIgnore]
        public int Hour { get; set; }

        public string DayHour { get { return Date.Day + "/" + Hour; } }
    }
}
