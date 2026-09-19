using AgitekBackend.Data;
using AgitekBackend.DTOs.OrderTrackingLogs;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class OrderTrackingLogService : IOrderTrackingLogService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderTrackingLogService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderTrackingLogDTO>> GetAllAsync()
        {
            var logs = await _context.OrderTrackingLogs.ToListAsync();
            return _mapper.Map<IEnumerable<OrderTrackingLogDTO>>(logs);
        }

        public async Task<OrderTrackingLogDTO?> GetByIdAsync(int id)
        {
            var log = await _context.OrderTrackingLogs.FindAsync(id);
            if (log == null) throw new NotFoundException($"Log with id {id} not found.");
            return _mapper.Map<OrderTrackingLogDTO>(log);
        }

        public async Task<OrderTrackingLogDTO> CreateAsync(CreateOrderTrackingLogDTO dto)
        {
            var log = _mapper.Map<OrderTrackingLog>(dto);
            _context.OrderTrackingLogs.Add(log);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderTrackingLogDTO>(log);
        }

        public async Task<OrderTrackingLogDTO?> UpdateAsync(int id, UpdateOrderTrackingLogDTO dto)
        {
            var log = await _context.OrderTrackingLogs.FindAsync(id);
            if (log == null) throw new NotFoundException($"Log with id {id} not found.");

            _mapper.Map(dto, log);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderTrackingLogDTO>(log);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var log = await _context.OrderTrackingLogs.FindAsync(id);
            if (log == null) throw new NotFoundException($"Log with id {id} not found.");

            _context.OrderTrackingLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




