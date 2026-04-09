using AutoMapper;
using System;
using WMS.LineCallInputModule.Domain;

namespace WMS.LineCallInputModule.Application
{
    //这是定义映射的。
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<LineCallInputOrder, LineCallInputOrderDto>();
            CreateMap<LineCallOutputOrder, LineCallOutputOrderDto>();
        }
    }
}
