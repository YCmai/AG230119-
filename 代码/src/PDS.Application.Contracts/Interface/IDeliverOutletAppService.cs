using PDS.Application.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace PDS.Application.Controcts
{
    public interface IDeliverOutletAppService : IApplicationService   
    {
        Task AutoBindingNearCageCar();
        Task<CommonResponseDto> ClearPackagesAsync(ClearPackagesInput clearPackagesInput);
    }
}
