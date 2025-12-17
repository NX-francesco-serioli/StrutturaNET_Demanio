using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdspMds.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity => entity.ToTable("AspNetUsers", "auth"));
        builder.Entity<IdentityRole>(entity => entity.ToTable("AspNetRoles", "auth"));
        builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("AspNetUserRoles", "auth"));
        builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("AspNetUserClaims", "auth"));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("AspNetUserLogins", "auth"));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("AspNetRoleClaims", "auth"));
        builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("AspNetUserTokens", "auth"));

        builder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions", "auth");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Permission).IsRequired().HasMaxLength(128);
            entity.HasIndex(p => new { p.UserId, p.Permission }).IsUnique();
            entity
                .HasOne(p => p.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
