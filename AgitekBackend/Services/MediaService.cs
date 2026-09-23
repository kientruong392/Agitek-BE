using AgitekBackend.Data;
using AgitekBackend.DTOs.Medias;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class MediaService : IMediaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MediaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MediaDTO>> GetAllAsync()
        {
            var medias = await _context.Medias.ToListAsync();
            return _mapper.Map<IEnumerable<MediaDTO>>(medias);
        }

        public async Task<MediaDTO?> GetByIdAsync(int id)
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null) throw new NotFoundException($"Media with id {id} not found");
            return _mapper.Map<MediaDTO>(media);
        }

        public async Task<MediaDTO> CreateAsync(CreateMediaDTO dto)
        {
            var media = _mapper.Map<Media>(dto);
            _context.Medias.Add(media);
            await _context.SaveChangesAsync();
            return _mapper.Map<MediaDTO>(media);
        }

        public async Task<MediaDTO?> UpdateAsync(int id, UpdateMediaDTO dto)
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null) throw new NotFoundException($"Media with id {id} not found");

            _mapper.Map(dto, media);
            await _context.SaveChangesAsync();
            return _mapper.Map<MediaDTO>(media);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null) throw new NotFoundException($"Media with id {id} not found");

            _context.Medias.Remove(media);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




