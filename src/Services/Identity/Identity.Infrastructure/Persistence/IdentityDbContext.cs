using Identity.Infrastructure.Identity.Authentication;
using Identity.Infrastructure.Identity.Contracts;
using Identity.Infrastructure.Identity.Models;
using Identity.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

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
