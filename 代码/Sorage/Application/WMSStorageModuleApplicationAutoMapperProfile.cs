using AutoMapper;
using System;
using WMS.StorageModule.Domain;

namespace WMS.StorageModule.Application
{
    //这是定义映射的。
    public class WMSStorageModuleApplicationAutoMapperProfile : Profile
    {
        public WMSStorageModuleApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<Storage, StorageDto>();
            CreateMap<WareHouse, WareHouseDto>();
            CreateMap<WareHouseArea, WareHouseAreaDto>().ForMember(dest => dest.Category, opt => opt.MapFrom(source => source.AreaCategory));
        }
    }
}
