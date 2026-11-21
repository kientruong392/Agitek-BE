using AgitekBackend.Common.Exceptions;
using AgitekBackend.Data;
using AgitekBackend.DTOs.AuditLogs;
using AgitekBackend.Models;
using AgitekBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgitekBackend.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuditLogPageDTO> GetPageAsync(AuditLogQueryDTO query)
        {
            var page = Math.Clamp(query.Page, 1, 1000000);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            if (query.From.HasValue && query.To.HasValue && query.From.Value.Date > query.To.Value.Date)
                throw new BadRequestException("Invalid date range");
            if (query.To.HasValue && query.To.Value.Date == DateTime.MaxValue.Date)
                throw new BadRequestException("Invalid end date");
            if (query.Search?.Length > 100)
                throw new BadRequestException("Search query is too long");
            var logs = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.EntityName))
                logs = logs.Where(log => log.EntityName == query.EntityName);
            if (!string.IsNullOrWhiteSpace(query.Action))
                logs = logs.Where(log => log.Action.StartsWith(query.Action));
            if (query.UserId.HasValue)
                logs = logs.Where(log => log.UserId == query.UserId);
            if (query.From.HasValue)
                logs = logs.Where(log => log.Timestamp >= query.From.Value.Date);
            if (query.To.HasValue)
                logs = logs.Where(log => log.Timestamp < query.To.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                logs = logs.Where(log => log.Action.Contains(search) || log.EntityName.Contains(search) ||
                    (log.User != null && ((log.User.Username != null && log.User.Username.Contains(search)) ||
                        (log.User.Email != null && log.User.Email.Contains(search)))));
            }

            var total = await logs.CountAsync();
            var items = await logs.OrderByDescending(log => log.Timestamp).ThenByDescending(log => log.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(log => new AuditLogDTO
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    UserId = log.UserId,
                    ActorName = log.User != null ? log.User.Username ?? log.User.Email : null,
                    EntityName = log.EntityName,
                    Action = log.Action
                })
                .ToListAsync();

            return new AuditLogPageDTO { Items = items, Total = total, Page = page, PageSize = pageSize };
        }

        public async Task<AuditLogDTO> GetByIdAsync(int id)
        {
            var log = await _context.AuditLogs.AsNoTracking()
                .Where(item => item.Id == id)
                .Select(item => new AuditLogDTO
                {
                    Id = item.Id,
                    Timestamp = item.Timestamp,
                    UserId = item.UserId,
                    ActorName = item.User != null ? item.User.Username ?? item.User.Email : null,
                    EntityName = item.EntityName,
                    Action = item.Action
                })
                .FirstOrDefaultAsync();
            return log ?? throw new NotFoundException("Log not found");
        }

        public async Task RecordStockReceiptAsync(int productId, int quantity, int? actorId, DateTime timestamp)
        {
            if (quantity <= 0) return;
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = actorId,
                EntityName = "Product",
                Action = $"StockIn:{productId}:{quantity}",
                Timestamp = timestamp
            });
            await _context.SaveChangesAsync();
        }

        public async Task RecordActionAsync(string entityName, string action)
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var id = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirstValue("id")
                ?? principal?.FindFirstValue("sub");
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = int.TryParse(id, out var actorId) ? actorId : null,
                EntityName = entityName,
                Action = action,
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<StockReceiptDTO>> GetStockReceiptsAsync(DateTime from, DateTime to)
        {
            var logs = await _context.AuditLogs.AsNoTracking()
                .Where(log => log.EntityName == "Product" && log.Action.StartsWith("StockIn:") &&
                    log.Timestamp >= from && log.Timestamp < to)
                .Select(log => new { log.Timestamp, log.Action })
                .ToListAsync();

            var receipts = new List<StockReceiptDTO>();
            foreach (var log in logs)
            {
                var parts = log.Action.Split(':');
                if (parts.Length != 3 || !int.TryParse(parts[1], out var productId) ||
                    !int.TryParse(parts[2], out var quantity) || quantity <= 0) continue;
                receipts.Add(new StockReceiptDTO { ProductId = productId, Quantity = quantity, Timestamp = log.Timestamp });
            }

            return receipts;
        }
    }
}
