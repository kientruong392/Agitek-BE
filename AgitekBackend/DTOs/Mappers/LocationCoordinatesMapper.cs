using AgitekBackend.DTOs.LocationCoordinates;
using LocationCoordinates = AgitekBackend.Models.LocationCoordinates;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    using LocationCoordinates = AgitekBackend.Models.LocationCoordinates;
    public class LocationCoordinatesMapper : AutoMapper.Profile
    {
        public LocationCoordinatesMapper()
        {
            CreateMap<CreateLocationCoordinatesDTO, LocationCoordinates>();
            CreateMap<UpdateLocationCoordinatesDTO, LocationCoordinates>();
            CreateMap<LocationCoordinates, LocationCoordinatesDTO>();
        }
    }
}



