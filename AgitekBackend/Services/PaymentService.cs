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
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            var payment = _mapper.Map<Payment>(dto);
            payment.Amount = order.TotalPrice;
            payment.Status = PaymentStatus.Pending;
            payment.ExpiresAt = payment.PaymentMethod == PaymentMethod.COD
                ? null
                : DateTime.UtcNow.AddHours(24);
            payment.Order = order;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<IEnumerable<PaymentDTO>> GetByOrderIdAsync(int orderId)
        {
            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDTO>>(payments);
        }

        public async Task<PaymentDTO> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment not found");
            return _mapper.Map<PaymentDTO>(payment);
        }

        public async Task<PaymentDTO?> GetPendingPaymentAsync(int orderId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId)
                .FirstOrDefaultAsync(p => p.Status == PaymentStatus.Pending);
            return payment == null ? null : _mapper.Map<PaymentDTO>(payment);
        }

        public async Task UpdateStatusAsync(int id, PaymentStatus status)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment not found");

            if (payment.Status != PaymentStatus.Pending ||
                (status != PaymentStatus.Success && status != PaymentStatus.Failed && status != PaymentStatus.Refunded))
            {
                throw new BadRequestException("Payment status can only change from Pending to Success, Failed or Refunded");
            }

            payment.Status = status;
            payment.PaidAt = status == PaymentStatus.Success || status == PaymentStatus.Refunded
                ? DateTime.UtcNow
                : null;
            payment.ExpiresAt = null;
            await _context.SaveChangesAsync();
        }
    }
}
