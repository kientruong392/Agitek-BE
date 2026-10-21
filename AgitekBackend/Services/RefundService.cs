using AgitekBackend.Common;
using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
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
        private readonly IMediaService _mediaService;

        public RefundService(
            AppDbContext context,
            IMapper mapper,
            IMediaService mediaService)
        {
            _context = context;
            _mapper = mapper;
            _mediaService = mediaService;
        }

        public async Task<IEnumerable<RefundDTO>> GetAllAsync()
        {
            var refunds = await RefundsWithDetails()
                .AsNoTracking()
                .OrderByDescending(refund => refund.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RefundDTO>>(refunds);
        }

        public async Task<RefundDTO> GetByIdAsync(int orderId, int productId)
        {
            var refund = await RefundsWithDetails()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.ProductId == productId);
            if (refund == null)
                throw new NotFoundException("Refund not found");

            return _mapper.Map<RefundDTO>(refund);
        }

        public async Task<RefundDTO> CreateAsync(CreateRefundDTO dto, int currentUserId)
        {
            if (dto.Medias == null || !dto.Medias.Any())
                throw new BadRequestException("Refund must have at least one media");

            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od => od.OrderId == dto.OrderId && od.ProductId == dto.ProductId);
            if (orderDetail == null)
                throw new NotFoundException("Order detail not found");

            if (orderDetail.Order.UserId != currentUserId)
                throw new BadRequestException("Order does not belong to the current user");

            if (orderDetail.Order.Status != OrderStatus.Delivered)
                throw new BadRequestException("Refunds are only available after the order is delivered");

            if (dto.Quantity > orderDetail.Quantity)
                throw new BadRequestException("Refund quantity cannot exceed order quantity");

            if (await _context.Refunds.AnyAsync(r =>
                r.OrderId == dto.OrderId && r.ProductId == dto.ProductId))
            {
                throw new BadRequestException("Order detail already has a refund");
            }

            var refund = _mapper.Map<Refund>(dto);
            refund.UserId = currentUserId;
            refund.Amount = orderDetail.UnitPrice * dto.Quantity;
            refund.Status = RefundStatus.Pending;
            refund.CreatedAt = DateTime.UtcNow;
            refund.UpdatedAt = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Refunds.Add(refund);
                _context.ChangeTracker.DetectChanges();

                foreach (var media in refund.Medias)
                {
                    media.RefundOrderId = refund.OrderId;
                    media.RefundProductId = refund.ProductId;
                    await _mediaService.ValidateMediaType(media);
                }

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

            if (string.IsNullOrWhiteSpace(dto.Response))
                throw new BadRequestException("Refund response cannot be empty");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(dto, refund);
                refund.Response = dto.Response.Trim();
                refund.ApprovedByUserId = currentUserId;
                refund.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (dto.Status == RefundStatus.Approved)
                {
                    _context.Payments.Add(new Payment
                    {
                        OrderId = refund.OrderId,
                        Amount = refund.Amount,
                        PaymentMethod = PaymentMethod.Bank,
                        Status = PaymentStatus.Refunded,
                        CreatedAt = DateTime.UtcNow,
                        PaidAt = DateTime.UtcNow,
                        GatewayResponse = $"Approved refund request for product #{refund.ProductId}"
                    });

                    var allOrderDetailsRefunded = await _context.OrderDetails
                        .Where(detail => detail.OrderId == refund.OrderId)
                        .AllAsync(detail => detail.Refund != null && detail.Refund.Status == RefundStatus.Approved);
                    if (allOrderDetailsRefunded)
                    {
                        var order = await _context.Orders.FirstOrDefaultAsync(order => order.Id == refund.OrderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Refunded;
                            order.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

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

        private IQueryable<Refund> RefundsWithDetails()
        {
            return _context.Refunds
                .Include(refund => refund.Medias)
                .Include(refund => refund.User)
                    .ThenInclude(user => user!.Profile)
                .Include(refund => refund.ApprovedByUser)
                    .ThenInclude(user => user!.Profile)
                .Include(refund => refund.OrderDetail)
                    .ThenInclude(detail => detail.Product)
                        .ThenInclude(product => product!.Images)
                .Include(refund => refund.OrderDetail)
                    .ThenInclude(detail => detail.Order)
                        .ThenInclude(order => order.User)
                            .ThenInclude(user => user.Profile)
                .AsSplitQuery();
        }
    }
}
