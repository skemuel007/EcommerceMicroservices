using AutoMapper;
using Catalog.API.Dtos.Request;
using Catalog.API.Entities;

namespace Catalog.API.Utils
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDto, Product>();
        }
    }
}
