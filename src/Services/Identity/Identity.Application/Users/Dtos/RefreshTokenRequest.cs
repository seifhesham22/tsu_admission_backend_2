using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Users.Dtos;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
