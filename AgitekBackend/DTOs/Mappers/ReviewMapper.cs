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
            CreateMap<UpdateReviewDTO, Review>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Review, ReviewDTO>();
        }
    }
}



