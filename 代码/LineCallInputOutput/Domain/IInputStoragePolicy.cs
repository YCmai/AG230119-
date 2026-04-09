using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.LineCallInputModule.Domain;
using WMS.MaterialModule.Domain;
using WMS.StorageModule.Domain;

namespace WMS.LineCallInputModule.Domain
{
    /// <summary>
    /// 入库储位选择策略
    /// </summary>
    public interface IInputStoragePolicy
    {
        Task<Storage> DecideStorageToDeliver(LineCallInputOrder lineCallInputOrderDto);
    }

    public interface IOutputStoragePolicy
    {
        Task<MaterialItem> DecideMaterialToOutput(LineCallOutputOrder lineCallInputOrderDto);
    }
}
