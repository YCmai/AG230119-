using AutoMapper;
using System;
using WMS.MaterialModule.Domain;

namespace WMS.MaterialModule.Application
{
    //这是定义映射的。
    public class WMSMaterialModuleApplicationAutoMapperProfile : Profile
    {
        public WMSMaterialModuleApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<MaterialItem, MaterialItemDto>();
            CreateMap<MaterialRecord, MaterialRecordDto>();
            CreateMap<MaterialModifyRecord, MaterialModifyRecordDto>();
            CreateMap<MaterialInfo, MaterialInfoDto>().ForMember(m => m.Sku, opt => opt.MapFrom(source => source.Id));
        }
    }
}
