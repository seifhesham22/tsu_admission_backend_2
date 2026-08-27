using Identity.Api.Persistence.Models;

namespace Identity.Api.Token;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);

    (string Token, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken();

    string Hash(string value);
}
