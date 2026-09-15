using AgitekBackend.DTOs.Orders;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class OrderMapper : AutoMapper.Profile
    {
        public OrderMapper()
        {
            // CreateDTO -> Model
            CreateMap<CreateOrderDTO, Order>();

            // UpdateDTO -> Model
            CreateMap<UpdateOrderDTO, Order>();

            // Model -> ViewDTO
            CreateMap<Order, OrderDTO>();
        }
    }
}



