using Identity.Api.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Identity.Api.Token;

public sealed class TempTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TempToken";

    private readonly AppIdentityDbContext _context;

    public TempTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AppIdentityDbContext context)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Temp-Token", out var header))
        {
            return AuthenticateResult.NoResult();
        }

        var token = header.ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.Fail("The temporary token is missing.");
        }

        var now = DateTime.UtcNow;
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.TempToken == token && x.TempTokenExpiresAtUtc > now);

        if (user is null)
        {
            return AuthenticateResult.Fail("The temporary token is invalid or has expired.");
        }

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            },
            SchemeName);

        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }
}
