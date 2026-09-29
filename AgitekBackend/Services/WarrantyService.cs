using AgitekBackend.Data;
using AgitekBackend.DTOs.Warranties;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class WarrantyService : IWarrantyService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WarrantyService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WarrantyDTO>> GetAllAsync()
        {
            var warranties = await _context.Warranties.ToListAsync();
            return _mapper.Map<IEnumerable<WarrantyDTO>>(warranties);
        }

        public async Task<WarrantyDTO?> GetByIdAsync(int id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null) throw new NotFoundException("Warranty not found");
            return _mapper.Map<WarrantyDTO>(warranty);
        }

        public async Task<WarrantyDTO> CreateAsync(CreateWarrantyDTO dto)
        {
            var warranty = _mapper.Map<Warranty>(dto);
            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();
            return _mapper.Map<WarrantyDTO>(warranty);
        }

        public async Task<WarrantyDTO?> UpdateAsync(int id, UpdateWarrantyDTO dto)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null) throw new NotFoundException("Warranty not found");

            _mapper.Map(dto, warranty);
            warranty.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<WarrantyDTO>(warranty);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null) throw new NotFoundException("Warranty not found");

            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




