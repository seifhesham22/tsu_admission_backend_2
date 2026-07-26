namespace Admission.Application.Sync.Dtos;

public sealed record CatalogueSyncResult(
    int Faculties,
    int EducationLevels,
    int DocumentTypes,
    int Programs);
