using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.MaterialModule.Application
{
    public class MoveStorageInput
    {
        /// <summary>
        /// 需要移动的储位
        /// </summary>
        public string StorageIdToMove { get; set; }
        /// <summary>
        /// 需要移动到的新的父项（如果为空，则表示移动到根）
        /// </summary>
        public string StorageIdOfNewParent { get; set; }
    }
}
