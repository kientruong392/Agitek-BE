using AgitekBackend.DTOs.Addresses;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class AddressMapper : AutoMapper.Profile
    {
        public AddressMapper()
        {
            CreateMap<CreateAddressDTO, Address>();
            CreateMap<UpdateAddressDTO, Address>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Address, AddressDTO>();
        }
    }
}



