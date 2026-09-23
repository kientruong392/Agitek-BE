using AgitekBackend.Data;
using AgitekBackend.DTOs.Addresses;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AddressService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDTO>> GetAllAsync()
        {
            var addresses = await _context.Addresses.ToListAsync();
            return _mapper.Map<IEnumerable<AddressDTO>>(addresses);
        }

        public async Task<AddressDTO?> GetByIdAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) throw new NotFoundException($"Address with id {id} not found");
            return _mapper.Map<AddressDTO>(address);
        }

        public async Task<AddressDTO> CreateAsync(CreateAddressDTO dto)
        {
            var address = _mapper.Map<Address>(dto);
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return _mapper.Map<AddressDTO>(address);
        }

        public async Task<AddressDTO?> UpdateAsync(int id, UpdateAddressDTO dto)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) throw new NotFoundException($"Address with id {id} not found");

            _mapper.Map(dto, address);
            await _context.SaveChangesAsync();
            return _mapper.Map<AddressDTO>(address);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) throw new NotFoundException($"Address with id {id} not found");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




