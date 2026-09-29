using AgitekBackend.Data;
using AgitekBackend.DTOs.AuditLogs;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AuditLogService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuditLogDTO>> GetAllAsync()
        {
            var logs = await _context.AuditLogs.ToListAsync();
            return _mapper.Map<IEnumerable<AuditLogDTO>>(logs);
        }

        public async Task<AuditLogDTO?> GetByIdAsync(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) throw new NotFoundException("Log not found");
            return _mapper.Map<AuditLogDTO>(log);
        }

        public async Task<AuditLogDTO> CreateAsync(CreateAuditLogDTO dto)
        {
            var log = _mapper.Map<AuditLog>(dto);
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return _mapper.Map<AuditLogDTO>(log);
        }

        public async Task<AuditLogDTO?> UpdateAsync(int id, UpdateAuditLogDTO dto)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) throw new NotFoundException("Log not found");

            _mapper.Map(dto, log);
            await _context.SaveChangesAsync();
            return _mapper.Map<AuditLogDTO>(log);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) throw new NotFoundException("Log not found");

            _context.AuditLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




