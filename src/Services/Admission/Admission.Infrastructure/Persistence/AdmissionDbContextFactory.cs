using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Admission.Infrastructure.Persistence;

public sealed class AdmissionDbContextFactory : IDesignTimeDbContextFactory<AdmissionDbContext>
{
    public AdmissionDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ADMISSION_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=admission;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AdmissionDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AdmissionDbContext(options);
    }
}
