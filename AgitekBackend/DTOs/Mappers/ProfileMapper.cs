    using AgitekBackend.DTOs.Profiles;
    using AutoMapper;
    using Profile = AgitekBackend.Models.Profile;

    namespace AgitekBackend.DTOs.Mappers
    {
        public class ProfileMapper : AutoMapper.Profile
        {
            public ProfileMapper()
            {
                CreateMap<CreateProfileDTO, Profile>();
                CreateMap<UpdateProfileDTO, Profile>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
                CreateMap<Profile, ProfileDTO>();
            }
        }
    }


