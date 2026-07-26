using Admission.Domain.Admissions;

namespace Admission.Application.Applicants.Dtos;

public sealed record AdmissionSummary(
    Guid Id,
    AdmissionStatus Status,
    Guid? ManagerId,
    string? ManagerFullName,
    IReadOnlyList<AdmissionProgramSummary> Programs);
