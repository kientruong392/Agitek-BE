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

        public async Task<IEnumerable<AddressDTO>> GetByUserIdAsync(int userId)
        {
            var addresses = await _context.Addresses
                .Include(a => a.Location)
                .Where(a => a.UserId == userId)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AddressDTO>>(addresses);
        }

        public async Task<AddressDTO?> GetByIdAsync(int id)
        {
            var address = await _context.Addresses
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (address == null) throw new NotFoundException("Address not found");
            return _mapper.Map<AddressDTO>(address);
        }

        public async Task<AddressDTO> CreateAsync(CreateAddressDTO dto, int currentUserId)
        {
            if (!await _context.Locations.AnyAsync(l => l.Id == dto.LocationId))
            {
                throw new BadRequestException("Location not found");
            }

            var existingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == currentUserId);

            if (existingAddress == null)
            {
                // First address of the user is always default
                if (!dto.IsDefault)
                {
                    throw new BadRequestException("The first address must be set as default");
                }
            }
            else if (dto.IsDefault)
            {
                // Unset current default before setting the new one
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == currentUserId && a.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            var address = _mapper.Map<Address>(dto);
            address.UserId = currentUserId;
            address.IsDefault = dto.IsDefault;

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(address.Id) ?? _mapper.Map<AddressDTO>(address);
        }

        public async Task<AddressDTO?> UpdateAsync(int id, UpdateAddressDTO dto)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) throw new NotFoundException("Address not found");

            if (dto.LocationId.HasValue && !await _context.Locations.AnyAsync(l => l.Id == dto.LocationId.Value))
            {
                throw new BadRequestException("Location not found");
            }

            if (dto.IsDefault == false)
            {
                throw new BadRequestException("IsDefault cannot be set to false");
            }

            if (dto.IsDefault == true)
            {
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == address.UserId && a.IsDefault && a.Id != address.Id);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            _mapper.Map(dto, address);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(address.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) throw new NotFoundException("Address not found");

            var userAddresses = await _context.Addresses
                .Where(a => a.UserId == address.UserId)
                .ToListAsync();

            if (address.IsDefault && userAddresses.Count > 1)
            {
                var nextDefault = await _context.Addresses
                    .Where(a => a.UserId == address.UserId && a.Id != address.Id)
                    .OrderBy(a => a.Id)
                    .FirstOrDefaultAsync();
                if (nextDefault != null)
                {
                    nextDefault.IsDefault = true;
                }
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
