using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;

namespace Files.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly FilesDbContext _context;

    public UnitOfWork(FilesDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
