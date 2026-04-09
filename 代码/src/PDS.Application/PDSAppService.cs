using System;
using System.Collections.Generic;
using System.Text;
using PDS.Localization;
using Volo.Abp.Application.Services;

namespace PDS
{
    /* Inherit your application services from this class.
     */
    public abstract class PDSAppService : ApplicationService
    {
        protected PDSAppService()
        {
            LocalizationResource = typeof(PDSResource);
        }
    }
}
