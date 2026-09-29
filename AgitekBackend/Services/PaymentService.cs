using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDTO> CreateAsync(CreatePaymentDTO dto)
        {
            if (dto.OrderId <= 0)
                throw new BadRequestException("Order Id cannot be empty");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            var payment = _mapper.Map<Payment>(dto);
            payment.Amount = order.TotalPrice;
            payment.Status = PaymentStatus.Pending;
            payment.ExpiresAt = DateTime.UtcNow.AddHours(24);
            payment.Order = order;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<IEnumerable<PaymentDTO>> GetByOrderIdAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        public async Task<PaymentDTO?> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment not found");

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO> UpdateStatusAsync(int id, PaymentStatus status)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment not found");

            if (payment.Status != PaymentStatus.Pending ||
                (status != PaymentStatus.Success && status != PaymentStatus.Failed))
            {
                throw new BadRequestException("Payment status can only change from Pending to Success or Failed");
            }

            payment.Status = status;
            payment.PaidAt = status == PaymentStatus.Success ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDTO>(payment);
        }
    }
}
