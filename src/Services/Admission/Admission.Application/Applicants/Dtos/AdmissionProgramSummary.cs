using Admission.Domain.Admissions;

namespace Admission.Application.Applicants.Dtos;

public sealed record AdmissionProgramSummary(
    Guid Id,
    Guid EducationProgramId,
    string EducationProgramName,
    ProgramPriority Priority);
