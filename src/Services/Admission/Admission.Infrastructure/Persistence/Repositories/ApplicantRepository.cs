using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Applicants;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

public sealed class ApplicantRepository : IApplicantRepository
{
    private readonly AdmissionDbContext _context;

    public ApplicantRepository(AdmissionDbContext context)
    {
        _context = context;
    }

    public Task<Applicant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Applicants.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Applicant?> GetByAuthIdAsync(Guid authId, CancellationToken cancellationToken = default) =>
        _context.Applicants.FirstOrDefaultAsync(x => x.AuthId == authId, cancellationToken);

    public Task<Applicant?> GetWithDocumentsByAuthIdAsync(
        Guid authId,
        CancellationToken cancellationToken = default) =>
        _context.Applicants
            .Include(x => x.Documents)
            .ThenInclude(document => (document as EducationDocument)!.DocumentType)
            .FirstOrDefaultAsync(x => x.AuthId == authId, cancellationToken);

    public Task<Applicant?> GetWithDocumentsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Applicants
            .Include(x => x.Documents)
            .ThenInclude(document => (document as EducationDocument)!.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Applicant?> GetFullProfileAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Applicants
            .Include(x => x.Documents)
            .ThenInclude(document => (document as EducationDocument)!.DocumentType)
            .Include(x => x.Admissions)
            .ThenInclude(admission => admission.Manager)
            .Include(x => x.Admissions)
            .ThenInclude(admission => admission.Programs)
            .ThenInclude(program => program.EducationProgram)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsForAuthIdAsync(Guid authId, CancellationToken cancellationToken = default) =>
        _context.Applicants.AnyAsync(x => x.AuthId == authId, cancellationToken);

    public void Add(Applicant applicant) => _context.Applicants.Add(applicant);
}
