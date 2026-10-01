using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.DTOs.Refunds;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Services
{
    public class RefundService : IRefundService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public RefundService(AppDbContext context, IMapper mapper, IOrderService orderService, IPaymentService paymentService)
        {
            _context = context;
            _mapper = mapper;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public async Task<IEnumerable<RefundDTO>> GetAllAsync()
        {
            var refunds = await _context.Refunds
                .AsNoTracking()
                .Include(r => r.Medias)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RefundDTO>>(refunds);
        }

        public async Task<RefundDTO> GetByIdAsync(int orderId, int productId)
        {
            var refund = await _context.Refunds
                .AsNoTracking()
                .Include(r => r.Medias)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId);
            if (refund == null)
                throw new NotFoundException("Refund not found");

            return _mapper.Map<RefundDTO>(refund);
        }

        public async Task<RefundDTO> CreateAsync(CreateRefundDTO dto, int currentUserId)
        {
            if (dto.Medias == null || !dto.Medias.Any())
                throw new BadRequestException("Refund must have at least one media");

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            if (order.UserId != currentUserId)
                throw new BadRequestException("Order does not belong to the current user");

            var orderDetail = order.OrderDetails
                .FirstOrDefault(od => od.ProductId == dto.ProductId);
            if (orderDetail == null)
                throw new NotFoundException("Order detail not found");

            if (dto.Quantity > orderDetail.Quantity)
                throw new BadRequestException("Refund quantity cannot exceed order quantity");

            if (await _context.Refunds.AnyAsync(r =>
                r.OrderId == dto.OrderId && r.ProductId == dto.ProductId))
            {
                throw new BadRequestException("Order product already has a refund");
            }

            var refund = _mapper.Map<Refund>(dto);
            refund.UserId = currentUserId;
            refund.Amount = orderDetail.UnitPrice * dto.Quantity;
            refund.Status = RefundStatus.Pending;
            refund.CreatedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;

            foreach (var media in refund.Medias)
            {
                media.RefundOrderId = refund.OrderId;
                media.RefundProductId = refund.ProductId;
                media.ReviewId = null;
                media.WarrantyId = null;
                media.OrderId = null;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Refunds.Add(refund);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(refund.OrderId, refund.ProductId);
        }

        public async Task<RefundDTO> UpdateAsync(int orderId, int productId, int currentUserId, ApproveRefundDTO dto)
        {
            var refund = await _context.Refunds.FirstOrDefaultAsync(r =>
                r.OrderId == orderId && r.ProductId == productId);
            if (refund == null)
                throw new NotFoundException("Refund not found");

            if (refund.Status != RefundStatus.Pending)
                throw new BadRequestException("Refund can only be updated while in Pending status");

            if (dto.Status != RefundStatus.Approved && dto.Status != RefundStatus.Rejected)
                throw new BadRequestException("Refund status must be Approved or Rejected");

            _mapper.Map(dto, refund);
            refund.ApprovedByUserId = currentUserId;
            refund.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (dto.Status == RefundStatus.Approved)
            {
                await _orderService.UpdateOrderStatusAsync(refund.OrderId, OrderStatus.Refunded);

                var refundPayment = await _paymentService.CreateAsync(new CreatePaymentDTO
                {
                    OrderId = refund.OrderId,
                    PaymentMethod = PaymentMethod.Bank
                });
                await _paymentService.UpdateStatusAsync(refundPayment.Id, PaymentStatus.Refunded);
            }

            return await GetByIdAsync(refund.OrderId, refund.ProductId);
        }
    }
}
