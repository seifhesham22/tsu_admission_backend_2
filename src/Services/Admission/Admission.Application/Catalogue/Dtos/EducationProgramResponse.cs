namespace Admission.Application.Catalogue.Dtos;

public sealed record EducationProgramResponse(
    Guid Id,
    string Name,
    string Code,
    string Language,
    string EducationForm,
    Guid FacultyId,
    string FacultyName,
    Guid EducationLevelId,
    string EducationLevelName);
