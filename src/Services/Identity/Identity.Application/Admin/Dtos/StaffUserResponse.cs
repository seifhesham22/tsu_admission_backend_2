namespace Identity.Application.Admin.Dtos;

public sealed record StaffUserResponse(Guid Id, string Email, string UserName, string Role);
