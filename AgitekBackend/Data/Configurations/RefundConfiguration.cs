using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.HasKey(r => new { r.OrderId, r.ProductId });

            builder.Property(r => r.Amount)
                   .HasColumnType("decimal(18, 2)");

            builder.HasOne(r => r.OrderDetail)
                   .WithOne(od => od.Refund)
                   .HasForeignKey<Refund>(r => new { r.OrderId, r.ProductId })
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.User)
                   .WithMany(u => u.Refunds)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ApprovedByUser)
                   .WithMany(u => u.ApprovedRefunds)
                   .HasForeignKey(r => r.ApprovedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}