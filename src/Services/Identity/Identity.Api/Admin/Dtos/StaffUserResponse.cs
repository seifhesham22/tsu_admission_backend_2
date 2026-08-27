namespace Identity.Api.Admin.Dtos;

public sealed record StaffUserResponse(Guid Id, string Email, string UserName, string Role);
