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
            CreateMap<UpdateAddressDTO, Address>();
            CreateMap<Address, AddressDTO>();
        }
    }
}



