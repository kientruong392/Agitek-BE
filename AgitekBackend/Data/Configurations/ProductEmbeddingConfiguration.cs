using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class ProductEmbeddingConfiguration : IEntityTypeConfiguration<ProductEmbedding>
    {
        public void Configure(EntityTypeBuilder<ProductEmbedding> builder)
        {
            builder.HasIndex(embedding => embedding.SourceHash);

            builder.HasOne(embedding => embedding.Product)
                .WithOne()
                .HasForeignKey<ProductEmbedding>(embedding => embedding.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
