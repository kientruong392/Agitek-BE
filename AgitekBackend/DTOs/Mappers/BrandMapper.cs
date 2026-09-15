using AgitekBackend.DTOs.Brands;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class BrandMapper : AutoMapper.Profile
    {
        public BrandMapper()
        {
            CreateMap<CreateBrandDTO, Brand>();
            CreateMap<UpdateBrandDTO, Brand>();
            CreateMap<Brand, BrandDTO>();
        }
    }
}



