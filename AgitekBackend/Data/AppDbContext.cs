using AgitekBackend.Models;
using AgitekBackend.Services;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PcBuild> PcBuilds { get; set; }
        public DbSet<ProductEmbedding> ProductEmbeddings { get; set; }
        public DbSet<SemanticSearchCache> SemanticSearchCaches { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OrderTrackingLog> OrderTrackingLogs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        private readonly AuditLogChangeTracker _auditLogChangeTracker;
        public bool AuditEnabled { get; set; } = true;

        public AppDbContext(DbContextOptions<AppDbContext> options, AuditLogChangeTracker auditLogChangeTracker) : base(options)
        {
            _auditLogChangeTracker = auditLogChangeTracker;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            if (!AuditEnabled)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (!acceptAllChangesOnSuccess)
                throw new NotSupportedException("Audit logging requires accepting tracked changes after saving");
            var changes = _auditLogChangeTracker.Capture(this);
            if (changes.Count == 0)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            await using var transaction = Database.CurrentTransaction == null
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;
            var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            AuditLogs.AddRange(_auditLogChangeTracker.CreateLogs(changes));
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
            return saved;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (!AuditEnabled)
                return base.SaveChanges(acceptAllChangesOnSuccess);
            if (!acceptAllChangesOnSuccess)
                throw new NotSupportedException("Audit logging requires accepting tracked changes after saving");
            var changes = _auditLogChangeTracker.Capture(this);
            if (changes.Count == 0)
                return base.SaveChanges(acceptAllChangesOnSuccess);

            using var transaction = Database.CurrentTransaction == null ? Database.BeginTransaction() : null;
            var saved = base.SaveChanges(acceptAllChangesOnSuccess);
            AuditLogs.AddRange(_auditLogChangeTracker.CreateLogs(changes));
            base.SaveChanges(acceptAllChangesOnSuccess);
            transaction?.Commit();
            return saved;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

