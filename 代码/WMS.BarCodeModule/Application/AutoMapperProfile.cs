using AutoMapper;
using System;
using WMS.BarCodeModule.Application.Dto;
using WMS.BarCodeModule.Domain;

namespace WMS.BarCodeModule.Application
{
    //这是定义映射的。
    public class WMSMaterialModuleApplicationAutoMapperProfile : Profile
    {
        public WMSMaterialModuleApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<BarCodeRecord, BarCodeRecordDto>();
        }
    }
}
