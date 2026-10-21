using AgitekBackend.Common;
using AgitekBackend.Common.Settings;
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
        private readonly VietQRSettings _vietQrSettings;

        public PaymentService(
            AppDbContext context,
            IMapper mapper,
            Microsoft.Extensions.Options.IOptions<VietQRSettings> vietQrSettings)
        {
            _context = context;
            _mapper = mapper;
            _vietQrSettings = vietQrSettings.Value;
        }

        public async Task<IEnumerable<PaymentDTO>> GetAllAsync()
        {
            var payments = await PaymentsWithOrderDetails()
                .AsNoTracking()
                .OrderByDescending(payment => payment.CreatedAt)
                .ToListAsync();

            return payments.Select(ToPaymentDTO);
        }

        public async Task<PaymentDTO> CreateAsync(CreatePaymentDTO dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new NotFoundException("Order not found");

            var payment = _mapper.Map<Payment>(dto);
            payment.Amount = order.TotalPrice;
            payment.Status = PaymentStatus.Pending;
            payment.ExpiresAt = payment.PaymentMethod == PaymentMethod.Bank
                ? DateTime.UtcNow.AddMinutes(15)
                : null;
            payment.Order = order;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return ToPaymentDTO(payment);
        }

        public async Task<IEnumerable<PaymentDTO>> GetByOrderIdAsync(int orderId)
        {
            var payments = await PaymentsWithOrderDetails()
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return payments.Select(ToPaymentDTO);
        }

        public async Task<PaymentDTO> GetByIdAsync(int id)
        {
            var payment = await PaymentsWithOrderDetails()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                throw new NotFoundException("Payment not found");
            return ToPaymentDTO(payment);
        }

        public async Task<PaymentDTO?> GetPendingPaymentAsync(int orderId)
        {
            var payment = await PaymentsWithOrderDetails()
                .Where(p => p.OrderId == orderId)
                .FirstOrDefaultAsync(p => p.Status == PaymentStatus.Pending);
            return payment == null ? null : ToPaymentDTO(payment);
        }

        public async Task<PublicPaymentDTO> GetPublicByOrderAsync(int orderId, string trackingNumber)
        {
            var payment = await PaymentsWithOrderDetails()
                .AsNoTracking()
                .Where(item => item.OrderId == orderId && item.Order.TrackingNumber == trackingNumber)
                .OrderByDescending(item => item.Id)
                .FirstOrDefaultAsync();
            if (payment == null || payment.PaymentMethod != PaymentMethod.Bank)
                throw new NotFoundException("Payment not found");

            var paymentDto = ToPaymentDTO(payment);
            return new PublicPaymentDTO
            {
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                ExpiresAt = payment.ExpiresAt,
                PaidAt = payment.PaidAt,
                OrderStatus = payment.Order.Status,
                QrCodeUrl = paymentDto.QrCodeUrl,
                BankId = paymentDto.BankId,
                AccountNumber = paymentDto.AccountNumber,
                AccountName = paymentDto.AccountName,
                TransferDescription = paymentDto.TransferDescription
            };
        }

        private PaymentDTO ToPaymentDTO(Payment payment)
        {
            var dto = _mapper.Map<PaymentDTO>(payment);
            var order = payment.Order;

            if (payment.PaymentMethod == PaymentMethod.Bank)
            {
                var description = Uri.EscapeDataString(order?.TrackingNumber ?? string.Empty);
                var accountName = Uri.EscapeDataString(_vietQrSettings.AccountName);
                var amount = payment.Amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                dto.QrCodeUrl = $"https://img.vietqr.io/image/{Uri.EscapeDataString(_vietQrSettings.BankId)}-{Uri.EscapeDataString(_vietQrSettings.AccountNumber)}-compact2.png?amount={amount}&addInfo={description}&accountName={accountName}";
                dto.BankId = _vietQrSettings.BankId;
                dto.AccountNumber = _vietQrSettings.AccountNumber;
                dto.AccountName = _vietQrSettings.AccountName;
                dto.TransferDescription = order?.TrackingNumber;
            }

            return dto;
        }

        private IQueryable<Payment> PaymentsWithOrderDetails()
        {
            return _context.Payments
                .Include(payment => payment.Order)
                    .ThenInclude(order => order.User)
                        .ThenInclude(user => user.Profile)
                .Include(payment => payment.Order)
                    .ThenInclude(order => order.OrderDetails)
                        .ThenInclude(detail => detail.Product)
                        .ThenInclude(product => product!.Images);
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
