using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Users.Dtos;

public sealed class VerifyTwoFactorRequest
{
    [Required]
    [MaxLength(16)]
    public string Code { get; init; } = string.Empty;
}
