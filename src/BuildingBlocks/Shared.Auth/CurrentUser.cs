namespace Shared.Auth;

public sealed record CurrentUser
{
    public required Guid Id { get; init; }

    public required string Role { get; init; }

    public string? Email { get; init; }

    public bool IsApplicant => string.Equals(Role, Roles.Applicant, StringComparison.Ordinal);

    public bool IsRegularManager => string.Equals(Role, Roles.RegularManager, StringComparison.Ordinal);

    public bool IsHeadManager => string.Equals(Role, Roles.HeadManager, StringComparison.Ordinal);

    public bool IsAdmin => string.Equals(Role, Roles.Admin, StringComparison.Ordinal);

    public bool IsManager => Roles.IsManagerRole(Role);
}
