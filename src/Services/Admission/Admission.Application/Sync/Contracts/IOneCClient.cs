using Admission.Application.Sync.Dtos;
namespace Admission.Application.Sync.Contracts;

public interface IOneCClient
{
    Task<IReadOnlyList<FacultySyncDto>> GetFacultiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationLevelSyncDto>> GetEducationLevelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationDocumentTypeSyncDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EducationProgramSyncDto>> GetProgramsAsync(CancellationToken cancellationToken = default);
}
