using System.ComponentModel.DataAnnotations;
using Admission.Domain.Applicants;

namespace Admission.Application.Applicants.Dtos;

public sealed class UpdateApplicantProfileRequest
{
    [MaxLength(200)]
    public string? FullName { get; init; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    public DateOnly? BirthDate { get; init; }

    public Gender? Gender { get; init; }

    [MaxLength(100)]
    public string? Citizenship { get; init; }

    [Phone]
    [MaxLength(32)]
    public string? PhoneNumber { get; init; }
}
