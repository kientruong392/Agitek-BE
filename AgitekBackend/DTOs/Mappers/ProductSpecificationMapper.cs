using AgitekBackend.DTOs.ProductSpecifications;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class ProductSpecificationMapper : AutoMapper.Profile
    {
        public ProductSpecificationMapper()
        {
            CreateMap<CreateProductSpecificationDTO, ProductSpecification>();
            CreateMap<UpdateProductSpecificationDTO, ProductSpecification>();
            CreateMap<ProductSpecification, ProductSpecificationDTO>();
        }
    }
}



