using Admission.Application.Catalogue.Dtos;
using Admission.Application.Catalogue.Contracts;
using Admission.Application.Catalogue.Services;
using Shared.Kernel.Pagination;

namespace Admission.Application.Persistence.Contracts;

public interface ICatalogueQueries
{
    Task<IReadOnlyList<FacultyResponse>> GetFacultiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationLevelResponse>> GetEducationLevelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetEducationFormsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetLanguagesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationDocumentTypeResponse>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<EducationProgramResponse>> SearchProgramsAsync(
        EducationProgramFilter filter,
        CancellationToken cancellationToken = default);
}
