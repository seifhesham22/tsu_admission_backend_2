using Admission.Domain.Catalogue;

namespace Admission.Application.Persistence.Contracts;

public interface ICatalogueRepository
{
    Task<EducationProgram?> GetProgramWithLevelAsync(Guid programId, CancellationToken cancellationToken = default);

    Task<EducationDocumentType?> GetDocumentTypeAsync(Guid documentTypeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationLevelCombination>> GetCombinationsAsync(CancellationToken cancellationToken = default);
}
