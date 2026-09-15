using AgitekBackend.Common;
using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class WarrantyConfiguration : IEntityTypeConfiguration<Warranty>
    {
        public void Configure(EntityTypeBuilder<Warranty> builder)
        {
            builder.HasIndex(w => w.Code).IsUnique();

            builder.HasOne(w => w.OrderDetail)
                   .WithMany(od => od.Warranties)
                   .HasForeignKey(w => new { w.OrderId, w.ProductId })
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}