using AgitekBackend.DTOs.Products;
using AgitekBackend.Models;
using AutoMapper;

namespace AgitekBackend.DTOs.Mappers
{
    public class ProductMapper : AutoMapper.Profile
    {
        public ProductMapper()
        {
            // CreateDTO -> Model
            CreateMap<CreateProductDTO, Product>();

            // UpdateDTO -> Model
            CreateMap<UpdateProductDTO, Product>();

            // Model -> ViewDTO
            CreateMap<Product, ProductDTO>();
        }
    }
}



