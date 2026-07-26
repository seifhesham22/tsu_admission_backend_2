using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Users.Dtos;

public sealed class VerifyTwoFactorRequest
{
    [Required]
    [MaxLength(16)]
    public string Code { get; init; } = string.Empty;
}
