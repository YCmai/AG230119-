using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PDS.Application.Interface
{

    public interface IPackageSortMatcher
    {
        Task<PackageSort> MatchAsync(string packageCode);
    }
}
