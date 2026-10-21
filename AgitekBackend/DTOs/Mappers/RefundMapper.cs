using AgitekBackend.DTOs.Refunds;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class RefundMapper : AutoMapper.Profile
    {
        public RefundMapper()
        {
            // Refund
            CreateMap<CreateRefundDTO, Refund>();
            CreateMap<ApproveRefundDTO, Refund>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Refund, RefundDTO>()
                .ForMember(dest => dest.Order, opts => opts.MapFrom(src => src.OrderDetail.Order));
        }
    }
}



