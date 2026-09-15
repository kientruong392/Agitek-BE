using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class LocationCoordinatesConfiguration : IEntityTypeConfiguration<LocationCoordinates>
    {
        public void Configure(EntityTypeBuilder<LocationCoordinates> builder)
        {
            builder.HasIndex(c => c.LocationCode).IsUnique();
        }
    }
}