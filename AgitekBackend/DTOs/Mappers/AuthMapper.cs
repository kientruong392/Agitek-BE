using AgitekBackend.DTOs.Auths;
using AgitekBackend.DTOs.Profiles;
using AgitekBackend.DTOs.Users;

namespace AgitekBackend.DTOs.Mappers
{
    public class AuthMapper : AutoMapper.Profile
    {
        public AuthMapper()
        {
            CreateMap<SignUpDTO, CreateUserDTO>();
            CreateMap<CreateStaffDTO, CreateUserDTO>();
        }
    }
}



