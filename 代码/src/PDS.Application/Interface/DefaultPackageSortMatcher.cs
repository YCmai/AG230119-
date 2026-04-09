using PDS.Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace PDS.Application.Interface
{
    public class DefaultPackageSortMatcher : IPackageSortMatcher,ITransientDependency
    {
        IRepository<PackageRegularFormat> _regularRepository;
        IRepository<PackageSort> _sortRepository;

        public DefaultPackageSortMatcher(IRepository<PackageRegularFormat> regularRepository, IRepository<PackageSort> sortRepository)
        {
            _regularRepository = regularRepository;
            _sortRepository = sortRepository;
        }
        public async Task<PackageSort> MatchAsync(string packageCode)
        {
            var regularFormats =await _regularRepository.GetListAsync();
            foreach (var regularFormat in regularFormats)
            {
                Regex regex = new Regex(regularFormat.RegularFormat);
                if (regex.Match(packageCode).Success)
                {
                  return  await _sortRepository.FirstOrDefaultAsync(m => m.Id == regularFormat.PackageSortId);
                }
            }
            return null;
        }
    }
}
