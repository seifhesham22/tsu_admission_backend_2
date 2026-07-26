using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=identity;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppIdentityDbContext(options);
    }
}
