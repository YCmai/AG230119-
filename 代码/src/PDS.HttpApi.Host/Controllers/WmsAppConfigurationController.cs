using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace WMS.Controllers
{
    public class WmsAppConfigurationController: AbpControllerBase
    {
        IAbpApplicationConfigurationAppService _applicationConfigurationAppService;
        IAbpAntiForgeryManager _antiForgeryManager;

        public WmsAppConfigurationController(
            IAbpApplicationConfigurationAppService applicationConfigurationAppService,
            IAbpAntiForgeryManager antiForgeryManager)
        {
            _applicationConfigurationAppService = applicationConfigurationAppService;
            _antiForgeryManager = antiForgeryManager;
        }
        //[HttpGet]
        //public async Task<ApplicationConfigurationDto> GetAsync()
        //{
        //    _antiForgeryManager.SetCookie();
        //    return (await _applicationConfigurationAppService.GetAsync()).IngoreLocation();
        //}
    }
}
