using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Files.Domain;
using Microsoft.EntityFrameworkCore;

namespace Files.Infrastructure.Persistence.Repositories;

public sealed class StoredFileRepository : IStoredFileRepository
{
    private readonly FilesDbContext _context;

    public StoredFileRepository(FilesDbContext context)
    {
        _context = context;
    }

    public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Files.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<StoredFile>> GetByApplicantAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default) =>
        await _context.Files
            .AsNoTracking()
            .Where(x => x.ApplicantId == applicantId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public void Add(StoredFile file) => _context.Files.Add(file);

    public void Remove(StoredFile file) => _context.Files.Remove(file);
}
