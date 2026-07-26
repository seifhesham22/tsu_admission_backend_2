using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

public sealed class CatalogueRepository : ICatalogueRepository
{
    private readonly AdmissionDbContext _context;

    public CatalogueRepository(AdmissionDbContext context)
    {
        _context = context;
    }

    public Task<EducationProgram?> GetProgramWithLevelAsync(
        Guid programId,
        CancellationToken cancellationToken = default) =>
        _context.EducationPrograms
            .Include(x => x.EducationLevel)
            .Include(x => x.Faculty)
            .FirstOrDefaultAsync(x => x.Id == programId, cancellationToken);

    public Task<EducationDocumentType?> GetDocumentTypeAsync(
        Guid documentTypeId,
        CancellationToken cancellationToken = default) =>
        _context.EducationDocumentTypes
            .Include(x => x.CurrentEducationLevel)
            .Include(x => x.NextEducationLevels)
            .FirstOrDefaultAsync(x => x.Id == documentTypeId, cancellationToken);

    public async Task<IReadOnlyList<EducationLevelCombination>> GetCombinationsAsync(
        CancellationToken cancellationToken = default) =>
        await _context.EducationLevelCombinations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
