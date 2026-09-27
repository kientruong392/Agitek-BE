using AgitekBackend.DTOs.Locations;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class LocationMapper : AutoMapper.Profile
    {
        public LocationMapper()
        {
            CreateMap<CreateLocationDTO, Location>();
            CreateMap<UpdateLocationDTO, Location>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Location, LocationDTO>();
        }
    }
}
