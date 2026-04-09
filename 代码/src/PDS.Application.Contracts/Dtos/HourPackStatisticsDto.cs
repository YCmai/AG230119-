using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Volo.Abp.Application.Dtos;


namespace PDS
{

    [Serializable]
    public class HourPackStatisticsDto
    {
        [JsonIgnore]
        public int Day { get; set; }
        [JsonIgnore]
        public int Hour { get; set; }
        public int PackageCount { get; set; }

        public string DayHour { get { return Day.ToString() + "日/" + Hour.ToString()+"时"; } }
    }
}
