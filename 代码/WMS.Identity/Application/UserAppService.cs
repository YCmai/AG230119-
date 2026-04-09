using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace WMS.Identity
{
    [Route("/api/wms/user")]
    public class UserAppService : ApplicationService
    {
        protected UserManager<IdentityUser> UserManager { get; }
        protected IRepository<IdentityUser> UserRepository { get; }

        public UserAppService(UserManager<IdentityUser> userManager, IRepository<IdentityUser> userRepository)
        {
            UserManager = userManager;
            UserRepository = userRepository;


        }
        [HttpPost("reset-password")]
        public async Task ResetPasswordAsync(ResetPasswordInput input)
        {
            var user = await UserRepository.GetAsync(m => m.Id == CurrentUser.Id.Value);
            var result = await UserManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
            Logger.LogInformation($"用户:{user.UserName}修改密码,结果:{(result.Succeeded ? "成功" : "失败")}", result);
            if (!result.Succeeded)
                throw new UserFriendlyException(result.Errors.FirstOrDefault()?.Description);
        }
    }
}
