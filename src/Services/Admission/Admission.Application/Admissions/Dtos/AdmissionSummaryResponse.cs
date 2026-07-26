using Admission.Domain.Admissions;

namespace Admission.Application.Admissions.Dtos;

public sealed record AdmissionSummaryResponse(
    Guid Id,
    Guid ApplicantId,
    string ApplicantFullName,
    string ApplicantEmail,
    AdmissionStatus Status,
    Guid? ManagerId,
    string? ManagerFullName,
    DateTime LastModifiedUtc,
    IReadOnlyList<AdmissionProgramResponse> Programs);
