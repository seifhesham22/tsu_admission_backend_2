using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed record AdmissionProgramResponse(
    Guid Id,
    Guid EducationProgramId,
    string EducationProgramName,
    ProgramPriority Priority);
