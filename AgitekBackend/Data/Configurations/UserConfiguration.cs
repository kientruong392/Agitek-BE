using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgitekBackend.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasOne(p => p.Profile)
                    .WithOne(u => u.User)
                    .HasForeignKey<Profile>(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.GrantedByUser)
                   .WithMany()
                   .HasForeignKey(u => u.GrantedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}