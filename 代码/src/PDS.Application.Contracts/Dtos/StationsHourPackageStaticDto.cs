using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class StationsHourPackageStaticDto
    {
        public string Station { get; set; }

        public List<HourPackStatisticsDto> Data { get; set; }
    }
}
