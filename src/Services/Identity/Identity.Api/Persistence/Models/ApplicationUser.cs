using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Persistence.Models;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? TempToken { get; set; }

    public DateTime? TempTokenExpiresAtUtc { get; set; }

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public bool HasValidTempToken(string token) =>
        !string.IsNullOrEmpty(TempToken) &&
        string.Equals(TempToken, token, StringComparison.Ordinal) &&
        TempTokenExpiresAtUtc > DateTime.UtcNow;

    public void ClearTempToken()
    {
        TempToken = null;
        TempTokenExpiresAtUtc = null;
    }

    public void ClearRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAtUtc = null;
    }
}
