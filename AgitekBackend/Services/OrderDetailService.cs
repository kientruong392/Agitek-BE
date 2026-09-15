using AgitekBackend.Data;
using AgitekBackend.DTOs.OrderDetails;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderDetailService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDetailDTO>> GetAllAsync()
        {
            var orderDetails = await _context.OrderDetails.ToListAsync();
            return _mapper.Map<IEnumerable<OrderDetailDTO>>(orderDetails);
        }

        public async Task<OrderDetailDTO?> GetByIdAsync(int orderId, int productId)
        {
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
            return orderDetail == null ? null : _mapper.Map<OrderDetailDTO>(orderDetail);
        }

        public async Task<OrderDetailDTO> CreateAsync(CreateOrderDetailDTO dto)
        {
            var orderDetail = _mapper.Map<OrderDetail>(dto);
            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDetailDTO>(orderDetail);
        }

        public async Task<OrderDetailDTO?> UpdateAsync(int orderId, int productId, UpdateOrderDetailDTO dto)
        {
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
            if (orderDetail == null) return null;

            _mapper.Map(dto, orderDetail);
            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDetailDTO>(orderDetail);
        }

        public async Task<bool> DeleteAsync(int orderId, int productId)
        {
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
            if (orderDetail == null) return false;

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


