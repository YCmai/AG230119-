using AutoMapper;
using PDS.Application.Contracts.Dtos;
using PDS.Domain.Entitys;
using PDS.Dtos;
using System;

namespace PDS
{
    //这是定义映射的。
    public class PDSApplicationAutoMapperProfile : Profile
    {
        public PDSApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<PackageRegularFormatCreate, PackageRegularFormat>()
             .ForMember(m => m.Id, opt => opt.MapFrom(s => Guid.NewGuid().ToString()));
            CreateMap<PackageRegularFormatUpdate, PackageRegularFormat>();
            CreateMap<PackageRegularFormat, PackageRegularFormatDto>()
                .ForMember(m => m.PackageSortName, opt => opt.MapFrom(s => s.PackageSort.Describe));

            CreateMap<PackageSort, GetAllPackageSortDto>();

            CreateMap<OperationStation, OperationStationDto>();

            CreateMap<DeliverOutlet, DeliverOutletDto>();

            CreateMap<DeliverProcessTask, PackageProcessDto>();

            CreateMap<CageCarStorage, GetCageCarStorageDto>();

            CreateMap<CageCarStorage, CageCarStorageDto>();
        }
    }
}
