using AgitekBackend.Data;
using AgitekBackend.DTOs.LocationCoordinates;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class LocationCoordinatesService : ILocationCoordinatesService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public LocationCoordinatesService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LocationCoordinatesDTO>> GetAllAsync()
        {
            var locations = await _context.LocationCoordinates.ToListAsync();
            return _mapper.Map<IEnumerable<LocationCoordinatesDTO>>(locations);
        }

        public async Task<LocationCoordinatesDTO?> GetByIdAsync(int id)
        {
            var location = await _context.LocationCoordinates.FindAsync(id);
            return location == null ? null : _mapper.Map<LocationCoordinatesDTO>(location);
        }

        public async Task<LocationCoordinatesDTO> CreateAsync(CreateLocationCoordinatesDTO dto)
        {
            var location = _mapper.Map<LocationCoordinates>(dto);
            _context.LocationCoordinates.Add(location);
            await _context.SaveChangesAsync();
            return _mapper.Map<LocationCoordinatesDTO>(location);
        }

        public async Task<LocationCoordinatesDTO?> UpdateAsync(int id, UpdateLocationCoordinatesDTO dto)
        {
            var location = await _context.LocationCoordinates.FindAsync(id);
            if (location == null) return null;

            _mapper.Map(dto, location);
            await _context.SaveChangesAsync();
            return _mapper.Map<LocationCoordinatesDTO>(location);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.LocationCoordinates.FindAsync(id);
            if (location == null) return false;

            _context.LocationCoordinates.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


