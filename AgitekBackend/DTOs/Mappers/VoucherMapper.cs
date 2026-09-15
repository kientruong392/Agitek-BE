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
            CreateMap<UpdateVoucherDTO, Voucher>();
            CreateMap<Voucher, VoucherDTO>();
        }
    }
}



