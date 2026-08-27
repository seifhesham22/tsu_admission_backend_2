using Identity.Api.Persistence.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Persistence;

public sealed class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.TempToken).HasMaxLength(64);
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(128);
            entity.HasIndex(x => x.RefreshTokenHash);
        });
    }
}
