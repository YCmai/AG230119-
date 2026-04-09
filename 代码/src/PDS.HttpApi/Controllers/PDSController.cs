using PDS.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace PDS.Controllers
{
    /* Inherit your controllers from this class.
     */
    public abstract class PDSController : AbpController
    {
        protected PDSController()
        {
            LocalizationResource = typeof(PDSResource);
        }
    }
}