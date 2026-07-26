using Admission.Application.Admissions.Contracts;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Microsoft.EntityFrameworkCore;

namespace Admission.Infrastructure.Persistence.Repositories;

public sealed class AdmissionRepository : IAdmissionRepository
{
    private readonly AdmissionDbContext _context;

    public AdmissionRepository(AdmissionDbContext context)
    {
        _context = context;
    }

    public Task<ApplicantAdmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Admissions
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ApplicantAdmission?> GetByApplicantIdAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default) =>
        _context.Admissions
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.ApplicantId == applicantId, cancellationToken);

    public Task<ApplicantAdmission?> GetWithProgramsByApplicantIdAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default) =>
        _context.Admissions
            .Include(x => x.Manager)
            .Include(x => x.Programs)
            .ThenInclude(program => program.EducationProgram)
            .FirstOrDefaultAsync(x => x.ApplicantId == applicantId, cancellationToken);

    public Task<ApplicantAdmission?> GetWithProgramsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _context.Admissions
            .Include(x => x.Manager)
            .Include(x => x.Programs)
            .ThenInclude(program => program.EducationProgram)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(ApplicantAdmission admission) => _context.Admissions.Add(admission);
}
