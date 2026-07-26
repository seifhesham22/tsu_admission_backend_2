using Admission.Application.Catalogue.Dtos;
using Shared.Kernel.Pagination;

namespace Admission.Application.Catalogue.Contracts;

public interface ICatalogueService
{
    Task<IReadOnlyList<FacultyResponse>> GetFacultiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationLevelResponse>> GetEducationLevelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetEducationFormsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetLanguagesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationDocumentTypeResponse>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<EducationProgramResponse>> SearchProgramsAsync(
        EducationProgramFilter filter,
        CancellationToken cancellationToken = default);

    IReadOnlyList<PriorityOption> GetPriorities();

    IReadOnlyList<AdmissionStatusOption> GetAdmissionStatuses();
}
