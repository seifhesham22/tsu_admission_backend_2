using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Files.Infrastructure.Persistence;

public sealed class FilesDbContextFactory : IDesignTimeDbContextFactory<FilesDbContext>
{
    public FilesDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("FILES_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=files;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<FilesDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FilesDbContext(options);
    }
}
