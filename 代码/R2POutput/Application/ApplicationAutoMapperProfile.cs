using AutoMapper;
using System;
using WMS.R2POutputModule.Domain;

namespace WMS.R2POutputModule.Application
{
    //这是定义映射的。
    public class ApplicationAutoMapperProfile : Profile
    {
        public ApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<R2POutputTask, R2POutputTaskDto>();
        }
    }
}
