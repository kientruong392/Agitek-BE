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
            CreateMap<UpdateProductSpecificationDTO, ProductSpecification>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProductSpecification, ProductSpecificationDTO>();
        }
    }
}



