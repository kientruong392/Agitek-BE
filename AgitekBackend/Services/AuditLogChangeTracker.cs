using System.Security.Claims;
using AgitekBackend.Data;
using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AgitekBackend.Services
{
    public class AuditLogChangeTracker
    {
        private static readonly HashSet<Type> AuditedTypes =
        [
            typeof(User), typeof(Profile), typeof(Address), typeof(Product),
            typeof(PcBuild), typeof(Brand), typeof(Category), typeof(Order),
            typeof(Payment), typeof(Refund), typeof(Warranty), typeof(Review),
            typeof(Voucher), typeof(Location), typeof(ProductImage),
            typeof(ProductSpecification), typeof(OrderDetail),
            typeof(OrderTrackingLog), typeof(Media)
        ];

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogChangeTracker(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<PendingChange> Capture(AppDbContext context)
        {
            context.ChangeTracker.DetectChanges();
            return context.ChangeTracker.Entries()
                .Where(entry => AuditedTypes.Contains(entry.Metadata.ClrType) &&
                    (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
                .Select(entry => new PendingChange(
                    entry,
                    entry.State,
                    entry.Properties.Where(property => property.IsModified && !property.Metadata.IsPrimaryKey())
                        .Select(property => property.Metadata.Name).ToList(),
                    GetKey(entry)))
                .Where(change => change.State != EntityState.Modified || change.Fields.Count > 0)
                .ToList();
        }

        public List<AuditLog> CreateLogs(IEnumerable<PendingChange> changes)
        {
            var actorId = GetActorId();
            var now = DateTime.Now;
            var logs = new List<AuditLog>();
            foreach (var change in changes)
            {
                var key = change.State == EntityState.Added ? GetKey(change.Entry) : change.Key;
                var verb = change.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Deleted => "Deleted",
                    _ => "Updated"
                };
                var fields = change.State == EntityState.Modified
                    ? $" [{string.Join(", ", change.Fields)}]"
                    : string.Empty;
                var action = $"{verb} #{key}{fields}";
                if (action.Length > 100) action = action[..100];

                var effectiveActorId = actorId;
                if (effectiveActorId == null && _httpContextAccessor.HttpContext != null)
                {
                    effectiveActorId = change.Entry.Entity switch
                    {
                        User user => user.Id,
                        Order order => order.UserId,
                        Profile profile => profile.UserId,
                        Address address => address.UserId,
                        Payment payment => payment.Order?.UserId,
                        Refund refund => refund.UserId,
                        Warranty warranty => warranty.UserId,
                        Review review => review.UserId,
                        PcBuild build => build.UserId,
                        _ => null
                    };
                    if (effectiveActorId == 0) effectiveActorId = null;
                }
                if (change.Entry.Entity is User deletedUser && change.State == EntityState.Deleted && effectiveActorId == deletedUser.Id)
                    effectiveActorId = null;

                logs.Add(new AuditLog
                {
                    Timestamp = now,
                    UserId = effectiveActorId,
                    EntityName = change.Entry.Metadata.ClrType.Name,
                    Action = action
                });
            }

            return logs;
        }

        private int? GetActorId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var id = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirstValue("id")
                ?? principal?.FindFirstValue("sub");
            return int.TryParse(id, out var actorId) ? actorId : null;
        }

        private static string GetKey(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key == null) return "?";
            return string.Join(",", key.Properties.Select(property =>
                entry.Property(property.Name).CurrentValue?.ToString() ?? "?"));
        }

        public record PendingChange(EntityEntry Entry, EntityState State, List<string> Fields, string Key);
    }
}
