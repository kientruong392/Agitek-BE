using AgitekBackend.DTOs.ProductImages;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class ProductImageMapper : AutoMapper.Profile
    {
        public ProductImageMapper()
        {
            CreateMap<CreateProductImageDTO, ProductImage>();
            CreateMap<UpdateProductImageDTO, ProductImage>();
            CreateMap<ProductImage, ProductImageDTO>();
        }
    }
}



