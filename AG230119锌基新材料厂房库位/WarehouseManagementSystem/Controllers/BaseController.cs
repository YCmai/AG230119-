using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WarehouseManagementSystem.Services;

namespace WarehouseManagementSystem.Controllers
{
    public class BaseController : Controller
    {
        private readonly ISystemExpirationService _expirationService;

        public BaseController(ISystemExpirationService expirationService)
        {
            _expirationService = expirationService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // 检查系统是否过期
            if (_expirationService.IsSystemExpired())
            {
                context.Result = new ContentResult
                {
                    ContentType = "text/html",
                    Content = GetExpirationPage(_expirationService.GetExpirationDate()),
                    StatusCode = 403
                };
                return;
            }

            // 如果剩余天数少于10天，显示警告
            int remainingDays = _expirationService.GetRemainingDays();
            if (remainingDays >= 0 && remainingDays <= 10)
            {
                ViewBag.WarningMessage = $"系统将在 {remainingDays} 天后到期，请及时续期！";
            }
        }

        private string GetExpirationPage(DateTime? expirationDate)
        {
            string formattedDate = expirationDate.HasValue 
                ? expirationDate.Value.ToString("yyyy年MM月dd日") 
                : "未知日期";

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                <title>系统已过期</title>
                <style>
                    body {{ font-family: 'Microsoft YaHei', Arial, sans-serif; background-color: #f8f9fa; text-align: center; padding: 50px; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                    h1 {{ color: #dc3545; }}
                    p {{ font-size: 18px; color: #343a40; line-height: 1.6; }}
                    .footer {{ margin-top: 30px; font-size: 14px; color: #6c757d; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>系统使用期限已过</h1>
                    <p>您的系统使用许可已于 {formattedDate} 到期。</p>
                    <p>请联系系统管理员续期或获取新的访问权限。</p>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} 仓库管理系统 - 所有权利保留
                    </div>
                </div>
            </body>
            </html>";
        }
    }
} 