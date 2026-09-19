using AgitekBackend.Data;
using AgitekBackend.DTOs.ProductSpecifications;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class ProductSpecificationService : IProductSpecificationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductSpecificationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductSpecificationDTO>> GetAllAsync()
        {
            var specs = await _context.ProductSpecifications.ToListAsync();
            return _mapper.Map<IEnumerable<ProductSpecificationDTO>>(specs);
        }

        public async Task<ProductSpecificationDTO?> GetByIdAsync(int id)
        {
            var spec = await _context.ProductSpecifications.FindAsync(id);
            if (spec == null) throw new NotFoundException($"Spec with id {id} not found.");
            return _mapper.Map<ProductSpecificationDTO>(spec);
        }

        public async Task<ProductSpecificationDTO> CreateAsync(CreateProductSpecificationDTO dto)
        {
            var spec = _mapper.Map<ProductSpecification>(dto);
            _context.ProductSpecifications.Add(spec);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductSpecificationDTO>(spec);
        }

        public async Task<ProductSpecificationDTO?> UpdateAsync(int id, UpdateProductSpecificationDTO dto)
        {
            var spec = await _context.ProductSpecifications.FindAsync(id);
            if (spec == null) throw new NotFoundException($"Spec with id {id} not found.");

            _mapper.Map(dto, spec);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductSpecificationDTO>(spec);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var spec = await _context.ProductSpecifications.FindAsync(id);
            if (spec == null) throw new NotFoundException($"Spec with id {id} not found.");

            _context.ProductSpecifications.Remove(spec);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




