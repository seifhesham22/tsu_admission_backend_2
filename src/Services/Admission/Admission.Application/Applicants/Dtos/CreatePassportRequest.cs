using System.ComponentModel.DataAnnotations;

namespace Admission.Application.Applicants.Dtos;

public sealed class CreatePassportRequest
{
    [Required]
    [MaxLength(32)]
    public string Series { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PlaceOfBirth { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string IssuedBy { get; init; } = string.Empty;

    [Required]
    public DateOnly IssueDate { get; init; }
}
