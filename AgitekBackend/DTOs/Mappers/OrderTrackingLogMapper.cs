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
            CreateMap<UpdateOrderTrackingLogDTO, OrderTrackingLog>();
            CreateMap<OrderTrackingLog, OrderTrackingLogDTO>();
        }
    }
}



