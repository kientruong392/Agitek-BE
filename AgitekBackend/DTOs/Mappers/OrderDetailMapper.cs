using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class OrderDetailMapper : AutoMapper.Profile
    {
        public OrderDetailMapper()
        {
            CreateMap<CreateOrderDetailDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailDTO, OrderDetail>();
            CreateMap<OrderDetail, OrderDetailDTO>();
        }
    }
}



