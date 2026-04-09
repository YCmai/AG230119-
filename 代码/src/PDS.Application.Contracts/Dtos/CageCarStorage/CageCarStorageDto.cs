using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace PDS
{
    public class CageCarStorageDto : EntityDto<string>
    {
        public string MapNodeName { get; set; }

        public CageCarCageCarStorageType StorageType { get; set; }
    }
}
