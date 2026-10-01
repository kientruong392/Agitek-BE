using AgitekBackend.DTOs.Medias;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class MediaMapper : AutoMapper.Profile
    {
        public MediaMapper()
        {
            CreateMap<CreateMediaDTO, Media>();
            CreateMap<Media, MediaDTO>();
        }
    }
}
