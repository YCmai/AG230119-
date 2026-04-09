using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RequestLocalization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.Localization;

namespace WMS.Controllers
{
    [Route("/api/language")]
    public class LanguageController : AbpController
    {
        protected IQueryStringCultureReplacement QueryStringCultureReplacement { get; }

        public LanguageController(IQueryStringCultureReplacement queryStringCultureReplacement)
        {
            QueryStringCultureReplacement = queryStringCultureReplacement;
        }

        [HttpGet("switch")]
        public virtual async Task<IActionResult> Switch(string culture, string uiCulture = "", string returnUrl = "")
        {
            if (!CultureHelper.IsValidCultureCode(culture))
            {
                throw new AbpException("Unknown language: " + culture + ". It must be a valid culture!");
            }

            AbpRequestCultureCookieHelper.SetCultureCookie(
                HttpContext,
                new RequestCulture(culture, uiCulture)
            );

            HttpContext.Items[AbpRequestLocalizationMiddleware.HttpContextItemName] = true;

            var context = new QueryStringCultureReplacementContext(HttpContext, new RequestCulture(culture, uiCulture), returnUrl);
            await QueryStringCultureReplacement.ReplaceAsync(context);

            if (!string.IsNullOrWhiteSpace(context.ReturnUrl))
            {
                return Redirect(GetRedirectUrl(context.ReturnUrl));
            }

            return Redirect("~/");
        }
    }
}
