using AgitekBackend.DTOs.Vouchers;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class VoucherMapper : AutoMapper.Profile
    {
        public VoucherMapper()
        {
            CreateMap<CreateVoucherDTO, Voucher>();
            CreateMap<UpdateVoucherDTO, Voucher>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Voucher, VoucherDTO>();
        }
    }
}


