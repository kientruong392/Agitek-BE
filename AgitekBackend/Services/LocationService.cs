using AgitekBackend.Common;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Locations;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public LocationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LocationDTO>> GetAllAsync()
        {
            var locations = await _context.Locations.ToListAsync();
            return _mapper.Map<IEnumerable<LocationDTO>>(locations);
        }

        public async Task<IEnumerable<LocationDTO>> GetAllWarehousesAsync()
        {
            var warehouses = await _context.Locations
                .Where(l => l.IsWarehouse)
                .ToListAsync();
            return _mapper.Map<IEnumerable<LocationDTO>>(warehouses);
        }

        public async Task<LocationDTO> GetByIdAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) throw new NotFoundException("Location not found");
            return _mapper.Map<LocationDTO>(location);
        }

        public async Task<LocationDTO> CreateAsync(CreateLocationDTO dto, CurrentUser? currentUser)
        {
            if (currentUser?.Role == RoleName.Manager && !dto.IsWarehouse)
            {
                throw new BadRequestException("Cannot create non-warehouse location");
            }

            var location = _mapper.Map<Location>(dto);
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(location.Id) ?? _mapper.Map<LocationDTO>(location);
        }

        public async Task<LocationDTO> UpdateAsync(int id, UpdateLocationDTO dto, CurrentUser? currentUser)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) throw new NotFoundException("Location not found");

            if (currentUser?.Role == RoleName.Manager && !location.IsWarehouse)
            {
                throw new BadRequestException("Cannot update non-warehouse location");
            }

            if (currentUser?.Role == RoleName.Manager && dto.IsWarehouse == false)
            {
                throw new BadRequestException("Cannot change a warehouse location to a non-warehouse location");
            }

            _mapper.Map(dto, location);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(location.Id);
        }

        public async Task<bool> DeleteAsync(int id, CurrentUser? currentUser)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) throw new NotFoundException("Location not found");

            if (currentUser?.Role == RoleName.Manager && !location.IsWarehouse)
            {
                throw new BadRequestException("Cannot delete non-warehouse location");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}