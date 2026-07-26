using Admission.Application.Admissions.Contracts;
using Admission.Application.Admissions.Dtos;
using Admission.Application.Admissions.Services;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Pagination;

namespace Admission.Infrastructure.Persistence.Queries;

public sealed class AdmissionQueries : IAdmissionQueries
{
    private readonly AdmissionDbContext _context;

    public AdmissionQueries(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdmissionSummaryResponse>> SearchAsync(
        AdmissionFilter filter,
        Guid currentManagerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<ApplicantAdmission> query = _context.Admissions
            .AsNoTracking()
            .Include(x => x.Applicant)
            .Include(x => x.Manager)
            .Include(x => x.Programs)
            .ThenInclude(program => program.EducationProgram);

        if (filter.EducationProgramId is { } programId)
        {
            query = query.Where(x => x.Programs.Any(p => p.EducationProgramId == programId));
        }

        if (filter.FacultyIds is { Count: > 0 })
        {
            query = query.Where(x =>
                x.Programs.Any(p => filter.FacultyIds.Contains(p.EducationProgram.FacultyId)));
        }

        if (filter.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (filter.OnlyUnassigned)
        {
            query = query.Where(x => x.ManagerId == null);
        }

        if (filter.OnlyMine)
        {
            query = query.Where(x => x.ManagerId == currentManagerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Applicant.FullName, pattern) ||
                EF.Functions.ILike(x.Applicant.Email, pattern));
        }

        var total = await query.CountAsync(cancellationToken);

        var admissions = await query
            .OrderByDescending(x => x.LastModifiedUtc)
            .ThenBy(x => x.Id)
            .Skip(filter.Page.Skip)
            .Take(filter.Page.PageSize)
            .ToListAsync(cancellationToken);

        var items = admissions
            .Select(x => new AdmissionSummaryResponse(
                x.Id,
                x.ApplicantId,
                x.Applicant.FullName,
                x.Applicant.Email,
                x.Status,
                x.ManagerId,
                x.Manager?.FullName,
                x.LastModifiedUtc,
                x.Programs
                    .OrderBy(program => program.Priority)
                    .Select(program => new AdmissionProgramResponse(
                        program.Id,
                        program.EducationProgramId,
                        program.EducationProgram.Name,
                        program.Priority))
                    .ToList()))
            .ToList();

        return PagedResult<AdmissionSummaryResponse>.Create(items, total, filter.Page);
    }

    public async Task<IReadOnlyList<SelectedProgramResponse>> GetSelectedProgramsAsync(
        Guid applicantId,
        CancellationToken cancellationToken = default) =>
        await _context.AdmissionPrograms
            .AsNoTracking()
            .Include(x => x.EducationProgram)
            .ThenInclude(program => program.Faculty)
            .Include(x => x.EducationProgram)
            .ThenInclude(program => program.EducationLevel)
            .Where(x => _context.Admissions
                .Any(admission => admission.Id == x.ApplicantAdmissionId &&
                                  admission.ApplicantId == applicantId))
            .OrderBy(x => x.Priority)
            .Select(x => new SelectedProgramResponse(
                x.Id,
                x.EducationProgramId,
                x.EducationProgram.Name,
                x.EducationProgram.Faculty.Name,
                x.EducationProgram.EducationLevel.Name,
                x.Priority))
            .ToListAsync(cancellationToken);
}
