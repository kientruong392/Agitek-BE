using AgitekBackend.DTOs.Warranties;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class WarrantyMapper : AutoMapper.Profile
    {
        public WarrantyMapper()
        {
            CreateMap<CreateWarrantyDTO, Warranty>();
            CreateMap<UpdateWarrantyDTO, Warranty>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Warranty, WarrantyDTO>();
        }
    }
}



