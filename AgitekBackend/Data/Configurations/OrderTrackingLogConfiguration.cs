using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class OrderTrackingLogConfiguration : IEntityTypeConfiguration<OrderTrackingLog>
    {
        public void Configure(EntityTypeBuilder<OrderTrackingLog> builder)
        {
            builder.HasOne(t => t.LocationCoordinates)
                   .WithMany(l => l.OrderTrackingLogs)
                   .HasForeignKey(t => t.LocationCoordinatesId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}