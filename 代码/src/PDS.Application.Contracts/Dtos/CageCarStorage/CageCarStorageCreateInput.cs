using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDS
{
    public class CageCarStorageCreateInput
    {
        public string MapNodeName { get; set; }

        public CageCarCageCarStorageType StorageType { get; set; }
    }
}
