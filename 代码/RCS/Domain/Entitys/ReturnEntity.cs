using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rcs.Domain.Entitys
{
    public class ReturnEntity
    {
        public int Status { get; set; } = 0;
        public string Message { get; set; } = "处理异常,任务生成不成功";
    }

}
