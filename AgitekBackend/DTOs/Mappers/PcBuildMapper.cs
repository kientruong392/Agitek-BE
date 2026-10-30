using AgitekBackend.DTOs.PcBuilds;
using AgitekBackend.Models;

namespace AgitekBackend.DTOs.Mappers
{
    public class PcBuildMapper : AutoMapper.Profile
    {
        public PcBuildMapper()
        {
            CreateMap<SavePcBuildDTO, PcBuild>()
                .ForMember(destination => destination.Products, options => options.Ignore());
            CreateMap<PcBuild, PcBuildDTO>()
                .ForMember(destination => destination.ProductIds,
                    options => options.MapFrom(source => source.Products.Select(product => product.Id)));
        }
    }
}
