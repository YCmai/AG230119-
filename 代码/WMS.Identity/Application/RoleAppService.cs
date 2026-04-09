using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;

namespace WMS.Identity
{
    [Route("/api/wms/role")]
    public class RoleAppService: ApplicationService
    {
        protected IdentityRoleManager RoleManager { get; }
        protected IIdentityRoleRepository RoleRepository { get; }
        protected IIdentityUserRepository UserRepository { get; }

        public RoleAppService(IdentityRoleManager roleManager, IIdentityRoleRepository roleRepository, IIdentityUserRepository userRepository)
        {
            RoleManager = roleManager;
            RoleRepository = roleRepository;
            UserRepository = userRepository;
        }
        [HttpGet("get/{id}")]
        public virtual async Task<WmsRoleDto> GetAsync(Guid id)
        {
            var role = await RoleManager.GetByIdAsync(id);
            role.GetProperty("Creator", "admin");
            role.GetProperty("CreateTime", DateTime.Now);
            return ObjectMapper.Map<IdentityRole, WmsRoleDto>(role);
        }
        [HttpGet("page")]
        [Authorize(IdentityPermissions.Roles.Default)]
        public virtual async Task<PagedResultDto<WmsRoleDto>> GetListAsync(GetIdentityRolesInput input)
        {
            var list = await RoleRepository.GetListAsync(input.Sorting, input.MaxResultCount, input.SkipCount, input.Filter);
            var totalCount = await RoleRepository.GetCountAsync(input.Filter);

            return new PagedResultDto<WmsRoleDto>(
                totalCount,
                ObjectMapper.Map<List<IdentityRole>, List<WmsRoleDto>>(list)
                );
        }
        //[HttpGet("all")]
        //public virtual async Task<List<HBRoleDto>> GetAllAsync()
        //{
        //    return null;
        //}
        [HttpPost("create")]
        [Authorize(IdentityPermissions.Roles.Create)]
        public virtual async Task<WmsRoleDto> CreateAsync(CreateRoleDto input)
        {
            var role = new IdentityRole(
               GuidGenerator.Create(),
               input.Name,
               CurrentTenant.Id
           )
            {
                IsDefault = input.IsDefault,
                IsPublic = input.IsPublic
            };

            role.SetProperty("Description", input.Description);
            role.SetProperty("Remark", input.Remark);
            role.SetProperty("Creator", CurrentUser.Name);
            role.SetProperty("CreateTime", DateTime.Now);
            //input.MapExtraPropertiesTo(role);

            await RoleManager.CreateAsync(role);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<IdentityRole, WmsRoleDto>(role);
        }
        [HttpPut("update/{id}")]
        [Authorize(IdentityPermissions.Roles.Update)]
        public virtual async Task<WmsRoleDto> UpdateAsync(Guid id, UpdateRoleDto input)
        {
            var role = await RoleRepository.GetAsync(id);
            role.SetProperty("Description", input.Description);
            role.SetProperty("Remark", input.Remark);
            return ObjectMapper.Map<IdentityRole, WmsRoleDto>(await RoleRepository.UpdateAsync(role));
        }
        [Authorize(IdentityPermissions.Roles.Delete)]
        [HttpDelete("delete/{id}")]
        public virtual async Task DeleteAsync(Guid id)
        {
            var roleUserCount = await UserRepository.GetCountAsync(roleId: id);
            if (roleUserCount != 0)
                throw new UserFriendlyException(L["Error:Role.User.Exist"]);
            var role = await RoleRepository.GetAsync(id);
            await RoleManager.DeleteAsync(role);
        }
    }
}
