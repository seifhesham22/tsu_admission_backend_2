using Files.Application.Persistence.Contracts;
using Files.Application.Storage.Contracts;
using Files.Domain;
using Microsoft.EntityFrameworkCore;

namespace Files.Infrastructure.Persistence.Repositories;

public sealed class AdmissionAccessRepository : IAdmissionAccessRepository
{
    private readonly FilesDbContext _context;

    public AdmissionAccessRepository(FilesDbContext context)
    {
        _context = context;
    }

    public Task<AdmissionAccess?> GetAsync(Guid applicantId, CancellationToken cancellationToken = default) =>
        _context.AdmissionAccess.FirstOrDefaultAsync(x => x.ApplicantId == applicantId, cancellationToken);

    public void Add(AdmissionAccess access) => _context.AdmissionAccess.Add(access);
}
