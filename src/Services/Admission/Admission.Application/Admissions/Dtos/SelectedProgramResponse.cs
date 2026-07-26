using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed record SelectedProgramResponse(
    Guid Id,
    Guid EducationProgramId,
    string EducationProgramName,
    string FacultyName,
    string EducationLevelName,
    ProgramPriority Priority);
