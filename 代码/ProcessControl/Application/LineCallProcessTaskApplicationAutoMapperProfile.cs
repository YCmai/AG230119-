using AutoMapper;
using System;
using WMS.LineCallProcessTaskModule.Domain;

namespace WMS.LineCallProcessTaskModule.Application
{
    //这是定义映射的。
    public class WMSProcessControlModuleApplicationAutoMapperProfile : Profile
    {
        public WMSProcessControlModuleApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<LineCallInputTask, LineCallInputTaskDto>();
        }
    }
}
