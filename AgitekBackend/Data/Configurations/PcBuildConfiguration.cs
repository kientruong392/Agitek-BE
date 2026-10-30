using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class PcBuildConfiguration : IEntityTypeConfiguration<PcBuild>
    {
        public void Configure(EntityTypeBuilder<PcBuild> builder)
        {
            builder.HasIndex(build => new { build.UserId, build.CreatedAt });
            builder.HasOne(build => build.User).WithMany().HasForeignKey(build => build.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(build => build.Products).WithMany(product => product.PcBuilds)
                .UsingEntity<Dictionary<string, object>>(
                    "PcBuildProduct",
                    right => right.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<PcBuild>().WithMany().HasForeignKey("PcBuildId").OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("PcBuildItems");
                        join.HasKey("PcBuildId", "ProductId");
                        join.HasIndex("ProductId");
                    });
        }
    }
}
