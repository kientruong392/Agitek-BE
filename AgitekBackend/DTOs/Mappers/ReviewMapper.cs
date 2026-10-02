using AgitekBackend.DTOs.Reviews;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class ReviewMapper : AutoMapper.Profile
    {
        public ReviewMapper()
        {
            CreateMap<CreateReviewDTO, Review>();
            CreateMap<UpdateReviewDTO, Review>()
                .ForMember(dest => dest.Medias, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ReplyReviewDTO, Review>();
            CreateMap<Review, ReviewDTO>();
        }
    }
}
