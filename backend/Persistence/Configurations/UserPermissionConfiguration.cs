using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdSPMdS.DemanioDigitale.Persistence.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions", "auth");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Permission).IsRequired().HasMaxLength(128);
        builder.HasIndex(p => new { p.UserId, p.Permission }).IsUnique();
        builder
            .HasOne(p => p.User)
            .WithMany(u => u.Permissions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
