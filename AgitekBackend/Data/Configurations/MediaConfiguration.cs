using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.HasOne(m => m.Review)
                   .WithMany(r => r.Media)
                   .HasForeignKey(m => m.ReviewId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Warranty)
                   .WithMany(w => w.Media)
                   .HasForeignKey(m => m.WarrantyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}