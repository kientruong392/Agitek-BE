using AgitekBackend.DTOs.OrderTrackingLogs;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class OrderTrackingLogMapper : AutoMapper.Profile
    {
        public OrderTrackingLogMapper()
        {
            CreateMap<CreateOrderTrackingLogDTO, OrderTrackingLog>();
            CreateMap<UpdateOrderTrackingLogDTO, OrderTrackingLog>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderTrackingLog, OrderTrackingLogDTO>();
        }
    }
}



