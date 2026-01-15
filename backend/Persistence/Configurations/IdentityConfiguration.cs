using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdSPMdS.DemanioDigitale.Persistence.Configurations;

public class IdentityConfiguration :
    IEntityTypeConfiguration<ApplicationUser>,
    IEntityTypeConfiguration<IdentityRole>,
    IEntityTypeConfiguration<IdentityUserRole<string>>,
    IEntityTypeConfiguration<IdentityUserClaim<string>>,
    IEntityTypeConfiguration<IdentityUserLogin<string>>,
    IEntityTypeConfiguration<IdentityRoleClaim<string>>,
    IEntityTypeConfiguration<IdentityUserToken<string>>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.ToTable("AspNetRoles", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.ToTable("AspNetUserRoles", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        builder.ToTable("AspNetUserClaims", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    {
        builder.ToTable("AspNetUserLogins", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
    {
        builder.ToTable("AspNetRoleClaims", "auth");
    }

    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
    {
        builder.ToTable("AspNetUserTokens", "auth");
    }
}
