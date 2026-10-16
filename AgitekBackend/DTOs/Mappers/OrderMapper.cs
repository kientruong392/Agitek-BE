using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.DTOs.OrderTrackingLogs;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class OrderMapper : AutoMapper.Profile
    {
        public OrderMapper()
        {
            CreateMap<CreateOrderDTO, Order>();
            CreateMap<CreateGuestOrderDTO, Order>();
            CreateMap<UpdateOrderDTO, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.PaymentId, opts => opts.MapFrom(src =>
                    src.Payments.OrderByDescending(payment => payment.Id).Select(payment => (int?)payment.Id).FirstOrDefault()));

            CreateMap<CreateOrderDetailDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailDTO, OrderDetail>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderDetail, OrderDetailDTO>()
                .ForMember(dest => dest.ProductName, opts => opts.MapFrom(src => src.Product == null ? string.Empty : src.Product.ProductName))
                .ForMember(dest => dest.ProductSlug, opts => opts.MapFrom(src => src.Product == null ? string.Empty : src.Product.Slug))
                .ForMember(dest => dest.ProductImageUrl, opts => opts.MapFrom(src => src.Product == null ? null : src.Product.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()))
                .ForMember(dest => dest.HasReview, opts => opts.MapFrom(src => src.Reviews != null && src.Reviews.Any()));

            CreateMap<CreateOrderTrackingLogDTO, OrderTrackingLog>();
            CreateMap<UpdateOrderTrackingLogDTO, OrderTrackingLog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderTrackingLog, OrderTrackingLogDTO>();
        }
    }
}
