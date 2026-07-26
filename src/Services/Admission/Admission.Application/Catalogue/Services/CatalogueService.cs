using Admission.Application.Admissions.Contracts;
using Admission.Application.Catalogue.Contracts;
using Admission.Application.Catalogue.Dtos;
using Admission.Application.Messaging.Contracts;
using Admission.Application.Persistence.Contracts;
using Admission.Domain.Admissions;
using Shared.Kernel.Pagination;

namespace Admission.Application.Catalogue.Services;

public sealed class CatalogueService : ICatalogueService
{
    private readonly ICatalogueQueries _queries;

    public CatalogueService(ICatalogueQueries queries)
    {
        _queries = queries;
    }

    public Task<IReadOnlyList<FacultyResponse>> GetFacultiesAsync(CancellationToken cancellationToken = default) =>
        _queries.GetFacultiesAsync(cancellationToken);

    public Task<IReadOnlyList<EducationLevelResponse>> GetEducationLevelsAsync(
        CancellationToken cancellationToken = default) =>
        _queries.GetEducationLevelsAsync(cancellationToken);

    public Task<IReadOnlyList<string>> GetEducationFormsAsync(CancellationToken cancellationToken = default) =>
        _queries.GetEducationFormsAsync(cancellationToken);

    public Task<IReadOnlyList<string>> GetLanguagesAsync(CancellationToken cancellationToken = default) =>
        _queries.GetLanguagesAsync(cancellationToken);

    public Task<IReadOnlyList<EducationDocumentTypeResponse>> GetDocumentTypesAsync(
        CancellationToken cancellationToken = default) =>
        _queries.GetDocumentTypesAsync(cancellationToken);

    public Task<PagedResult<EducationProgramResponse>> SearchProgramsAsync(
        EducationProgramFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return _queries.SearchProgramsAsync(filter, cancellationToken);
    }

    public IReadOnlyList<PriorityOption> GetPriorities() =>
        Enum.GetValues<ProgramPriority>()
            .Select(value => new PriorityOption((int)value, value.ToString()))
            .ToList();

    public IReadOnlyList<AdmissionStatusOption> GetAdmissionStatuses() =>
        Enum.GetValues<AdmissionStatus>()
            .Select(value => new AdmissionStatusOption((int)value, value.ToString()))
            .ToList();
}
