using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Admin.Dtos;

public sealed class CreateStaffUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
