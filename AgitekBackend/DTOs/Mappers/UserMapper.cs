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
            CreateMap<UpdateUserDTO, User>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UserDTO>();
            CreateMap<User, GrantedByUserDTO>();
        }
    }
}



