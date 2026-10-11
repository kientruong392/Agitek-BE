using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasIndex(o => o.TrackingNumber).IsUnique();

            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.ApprovedByUser)
                   .WithMany(u => u.ApprovedOrders)
                   .HasForeignKey(o => o.ApprovedByUserId);

            builder.HasOne(o => o.DeliveredByUser)
                   .WithMany(u => u.DeliveredOrders)
                   .HasForeignKey(o => o.DeliveredByUserId);

            builder.HasOne(o => o.Voucher)
                   .WithMany()
                   .HasForeignKey(o => o.VoucherId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}