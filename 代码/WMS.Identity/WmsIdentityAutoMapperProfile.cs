using AutoMapper;
using Volo.Abp.AuditLogging;
using IdentityRole = Volo.Abp.Identity.IdentityRole;

namespace WMS.Identity
{
    public class WmsIdentityAutoMapperProfile: Profile
    {
        public WmsIdentityAutoMapperProfile()
        {
            CreateMap<IdentityRole, WmsRoleDto>()
               .ForMember(m => m.Description, opt => opt.MapFrom(s => s.ExtraProperties.ContainsKey("Description") ? s.ExtraProperties["Description"] : ""))
               .ForMember(m => m.Remark, opt => opt.MapFrom(s => s.ExtraProperties.ContainsKey("Remark") ? s.ExtraProperties["Remark"] : ""))
               .ForMember(m => m.Creator, opt => opt.MapFrom(s => s.ExtraProperties.ContainsKey("Creator") ? s.ExtraProperties["Creator"] : ""))
               .ForMember(m => m.CreateTime, opt => opt.MapFrom(s => s.ExtraProperties.ContainsKey("CreateTime") ? ((DateTime)s.ExtraProperties["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""));
            CreateMap<AuditLog, AuditDto>();
        }
    }
}
