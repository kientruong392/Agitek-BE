using AgitekBackend.DTOs.Users;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class UserMapper : AutoMapper.Profile
    {
        public UserMapper()
        {
            // CreateDTO -> Model
            CreateMap<CreateUserDTO, User>();

            // UpdateDTO -> Model
            CreateMap<UpdateUserDTO, User>();

            // Model -> ViewDTO
            CreateMap<User, UserDTO>();
        }
    }
}



