using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Admin.Dtos;

public sealed class UpdateStaffUserRequest
{
    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(32)]
    public string? Role { get; init; }
}
