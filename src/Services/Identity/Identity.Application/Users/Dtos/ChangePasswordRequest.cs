using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Users.Dtos;

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;
}
