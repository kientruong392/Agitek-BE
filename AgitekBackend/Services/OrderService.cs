using AgitekBackend.Data;
using AgitekBackend.DTOs.Orders;
using AgitekBackend.Models;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllAsync()
        {
            var orders = await _context.Orders.ToListAsync();
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO?> GetByIdAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException($"Order with id {id} not found.");
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO> CreateAsync(CreateOrderDTO dto)
        {
            var order = _mapper.Map<Order>(dto);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<OrderDTO?> UpdateAsync(int id, UpdateOrderDTO dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException($"Order with id {id} not found.");

            _mapper.Map(dto, order);
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDTO>(order);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new NotFoundException($"Order with id {id} not found.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}




