using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Users.Dtos;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
