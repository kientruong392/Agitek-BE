using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class UserMapper : AutoMapper.Profile
    {
        public UserMapper()
        {
            CreateMap<CreateUserDTO, User>();
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash)));
            CreateMap<User, GrantedByUserDTO>();
        }
    }
}



