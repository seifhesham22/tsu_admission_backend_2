using Identity.Infrastructure.Identity.Models;
namespace Identity.Infrastructure.Identity.Contracts;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);

    (string Token, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken();

    string Hash(string value);
}
