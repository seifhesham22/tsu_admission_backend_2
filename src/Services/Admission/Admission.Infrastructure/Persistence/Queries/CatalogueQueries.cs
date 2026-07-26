using Admission.Application.Admissions.Contracts;
using Admission.Application.Catalogue.Contracts;
using Admission.Application.Catalogue.Dtos;
using Admission.Application.Catalogue.Services;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Catalogue;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Pagination;

namespace Admission.Infrastructure.Persistence.Queries;

public sealed class CatalogueQueries : ICatalogueQueries
{
    private readonly AdmissionDbContext _context;

    public CatalogueQueries(AdmissionDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FacultyResponse>> GetFacultiesAsync(
        CancellationToken cancellationToken = default) =>
        await _context.Faculties
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new FacultyResponse(x.Id, x.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EducationLevelResponse>> GetEducationLevelsAsync(
        CancellationToken cancellationToken = default) =>
        await _context.EducationLevels
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new EducationLevelResponse(x.Id, x.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetEducationFormsAsync(
        CancellationToken cancellationToken = default) =>
        await _context.EducationPrograms
            .AsNoTracking()
            .Where(x => x.EducationForm != string.Empty)
            .Select(x => x.EducationForm)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetLanguagesAsync(
        CancellationToken cancellationToken = default) =>
        await _context.EducationPrograms
            .AsNoTracking()
            .Where(x => x.Language != string.Empty)
            .Select(x => x.Language)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EducationDocumentTypeResponse>> GetDocumentTypesAsync(
        CancellationToken cancellationToken = default) =>
        await _context.EducationDocumentTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new EducationDocumentTypeResponse(x.Id, x.Name, x.CurrentEducationLevelId))
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<EducationProgramResponse>> SearchProgramsAsync(
        EducationProgramFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<EducationProgram> query = _context.EducationPrograms
            .AsNoTracking()
            .Include(x => x.Faculty)
            .Include(x => x.EducationLevel);

        if (filter.FacultyIds is { Count: > 0 })
        {
            query = query.Where(x => filter.FacultyIds.Contains(x.FacultyId));
        }

        if (filter.EducationLevelIds is { Count: > 0 })
        {
            query = query.Where(x => filter.EducationLevelIds.Contains(x.EducationLevelId));
        }

        if (filter.Languages is { Count: > 0 })
        {
            query = query.Where(x => filter.Languages.Contains(x.Language));
        }

        if (filter.EducationForms is { Count: > 0 })
        {
            query = query.Where(x => filter.EducationForms.Contains(x.EducationForm));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                EF.Functions.ILike(x.Code, pattern));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(filter.Page.Skip)
            .Take(filter.Page.PageSize)
            .Select(x => new EducationProgramResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Language,
                x.EducationForm,
                x.FacultyId,
                x.Faculty.Name,
                x.EducationLevelId,
                x.EducationLevel.Name))
            .ToListAsync(cancellationToken);

        return PagedResult<EducationProgramResponse>.Create(items, total, filter.Page);
    }
}
