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
            CreateMap<UpdateOrderDetailDTO, OrderDetail>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<OrderDetail, OrderDetailDTO>();
        }
    }
}



