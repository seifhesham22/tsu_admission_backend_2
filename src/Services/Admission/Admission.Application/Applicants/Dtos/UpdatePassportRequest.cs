using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Applicants.Dtos;

public sealed class UpdatePassportRequest
{
    [MaxLength(32)]
    public string? Series { get; init; }

    [MaxLength(200)]
    public string? PlaceOfBirth { get; init; }

    [MaxLength(200)]
    public string? IssuedBy { get; init; }

    public DateOnly? IssueDate { get; init; }
}
