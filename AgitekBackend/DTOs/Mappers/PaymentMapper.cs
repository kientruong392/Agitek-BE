using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class PaymentMapper : AutoMapper.Profile
    {
        public PaymentMapper()
        {
            CreateMap<CreatePaymentDTO, Payment>();
            CreateMap<Payment, PaymentDTO>()
                .ForMember(dest => dest.OrderUserId,
                    opt => opt.MapFrom(src => src.Order != null ? src.Order.UserId : 0));
        }
    }
}
