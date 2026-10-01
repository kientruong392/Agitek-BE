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
            // Order
            CreateMap<CreateOrderDTO, Order>();
            CreateMap<UpdateOrderDTO, Order>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Order, OrderDTO>();

            // OrderDetail
            CreateMap<CreateOrderDetailDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailDTO, OrderDetail>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderDetail, OrderDetailDTO>();

            // OrderTrackingLog
            CreateMap<CreateOrderTrackingLogDTO, OrderTrackingLog>();
            CreateMap<UpdateOrderTrackingLogDTO, OrderTrackingLog>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderTrackingLog, OrderTrackingLogDTO>();
        }
    }
}



