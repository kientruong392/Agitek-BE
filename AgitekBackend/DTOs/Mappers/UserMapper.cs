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
            CreateMap<User, UserDTO>();
            CreateMap<User, GrantedByUserDTO>();
        }
    }
}



