using Admission.Domain.Applicants;

namespace Admission.Application.Applicants.Dtos;

public sealed record ApplicantProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    DateOnly? BirthDate,
    Gender? Gender,
    string? Citizenship,
    string? PhoneNumber,
    DateTime LastModifiedUtc);
