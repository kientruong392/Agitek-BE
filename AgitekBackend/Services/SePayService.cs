using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgitekBackend.Common;
using AgitekBackend.Common.Settings;
using AgitekBackend.Data;
using AgitekBackend.DTOs.Payments;
using AgitekBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgitekBackend.Services
{
    public class SePayService : ISePayService
    {
        private static readonly Regex TrackingNumberPattern = new(@"AGT\d{6}[A-F0-9]{9}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly AppDbContext _context;
        private readonly SePaySettings _settings;

        public SePayService(
            AppDbContext context,
            IOptions<SePaySettings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }

        public async Task ProcessWebhookAsync(byte[] rawPayload, string? timestamp, string? signature)
        {
            var signatureFailure = ValidateSignature(rawPayload, timestamp, signature);
            if (signatureFailure != null)
                throw new UnauthorizedAccessException("Invalid SePay webhook signature");

            SePayWebhookDTO? payload;
            try
            {
                payload = JsonSerializer.Deserialize<SePayWebhookDTO>(rawPayload);
            }
            catch (JsonException)
            {
                throw new ArgumentException("Invalid SePay webhook payload");
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.EffectiveTransferType) || payload.EffectiveTransferAmount <= 0)
                throw new ArgumentException("Invalid SePay webhook payload");

            var rawPayloadText = Encoding.UTF8.GetString(rawPayload);
            if (!string.Equals(payload.EffectiveTransferType, "in", StringComparison.OrdinalIgnoreCase))
                return;

            var orderId = await ExtractOrderIdAsync(payload.Content) ?? await ExtractOrderIdAsync(payload.Code);
            if (!orderId.HasValue)
                return;

            var payment = await _context.Payments
                .Include(item => item.Order)
                .Where(item => item.OrderId == orderId.Value &&
                    item.PaymentMethod == PaymentMethod.Bank &&
                    item.Status == PaymentStatus.Pending)
                .OrderByDescending(item => item.Id)
                .FirstOrDefaultAsync();
            if (payment == null)
                return;

            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var pendingPayment = await _context.Payments
                .Include(item => item.Order)
                .FirstOrDefaultAsync(item => item.Id == payment.Id && item.Status == PaymentStatus.Pending);
            var now = DateTime.UtcNow;
            if (pendingPayment == null || pendingPayment.ExpiresAt.HasValue && pendingPayment.ExpiresAt.Value < now)
            {
                await transaction.RollbackAsync();
                return;
            }

            pendingPayment.GatewayResponse = rawPayloadText;
            if (payload.EffectiveTransferAmount >= pendingPayment.Amount)
            {
                pendingPayment.Status = PaymentStatus.Success;
                pendingPayment.PaidAt = now;
                pendingPayment.TransactionId = payload.EffectiveReferenceNumber;
                pendingPayment.ExpiresAt = null;
                pendingPayment.Order.Status = OrderStatus.Paid;
                pendingPayment.Order.UpdatedAt = now;
            }
            else
            {
                pendingPayment.Status = PaymentStatus.Failed;
                pendingPayment.ExpiresAt = null;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private string? ValidateSignature(byte[] rawPayload, string? timestamp, string? signature)
        {
            var secretKeyValue = _settings.SecretKey?.Trim();
            if (string.IsNullOrEmpty(secretKeyValue))
                return "secret missing";

            if (string.IsNullOrWhiteSpace(timestamp) ||
                !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixTimestamp))
                return "timestamp missing or invalid";

            DateTimeOffset requestTime;
            try
            {
                requestTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "timestamp invalid";
            }

            var timestampDifference = DateTimeOffset.UtcNow - requestTime;
            if (timestampDifference < TimeSpan.FromSeconds(-300) || timestampDifference > TimeSpan.FromSeconds(300))
                return "timestamp outside allowed window";

            if (string.IsNullOrWhiteSpace(signature))
                return "signature missing";

            var normalizedSignature = signature.Trim();
            const string signaturePrefix = "sha256=";
            if (!normalizedSignature.StartsWith(signaturePrefix, StringComparison.OrdinalIgnoreCase))
                return "signature malformed";

            byte[] suppliedHash;
            try
            {
                suppliedHash = Convert.FromHexString(normalizedSignature[signaturePrefix.Length..]);
            }
            catch (FormatException)
            {
                return "signature malformed";
            }

            if (suppliedHash.Length != SHA256.HashSizeInBytes)
                return "signature malformed";

            var canonicalTimestamp = unixTimestamp.ToString(CultureInfo.InvariantCulture);
            var timestampBytes = Encoding.UTF8.GetBytes($"{canonicalTimestamp}.");
            var signedPayload = new byte[timestampBytes.Length + rawPayload.Length];
            Buffer.BlockCopy(timestampBytes, 0, signedPayload, 0, timestampBytes.Length);
            Buffer.BlockCopy(rawPayload, 0, signedPayload, timestampBytes.Length, rawPayload.Length);

            var secretKey = Encoding.UTF8.GetBytes(secretKeyValue);
            var expectedHash = HMACSHA256.HashData(secretKey, signedPayload);
            return CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash)
                ? null
                : "signature mismatch";
        }

        private async Task<int?> ExtractOrderIdAsync(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var match = TrackingNumberPattern.Match(value);
            if (!match.Success)
                return null;

            return await _context.Orders
                .AsNoTracking()
                .Where(order => order.TrackingNumber == match.Value)
                .Select(order => (int?)order.Id)
                .FirstOrDefaultAsync();
        }
    }
}
