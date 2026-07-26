namespace Shared.Auth;

public static class Roles
{
    public const string Applicant = "Applicant";
    public const string RegularManager = "RegularManager";
    public const string HeadManager = "HeadManager";
    public const string Admin = "Admin";

    public const string AnyManager = $"{RegularManager},{HeadManager}";
    public const string AnyManagerOrAdmin = $"{RegularManager},{HeadManager},{Admin}";
    public const string AnyAuthenticated = $"{Applicant},{RegularManager},{HeadManager},{Admin}";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Applicant,
        RegularManager,
        HeadManager,
        Admin
    };

    public static bool IsManagerRole(string role) =>
        string.Equals(role, RegularManager, StringComparison.Ordinal) ||
        string.Equals(role, HeadManager, StringComparison.Ordinal);
}
