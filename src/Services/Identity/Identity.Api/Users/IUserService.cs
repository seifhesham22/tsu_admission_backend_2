using Identity.Api.Users.Dtos;

namespace Identity.Api.Users;

public interface IUserService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<TwoFactorChallengeResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthTokensResponse> VerifyTwoFactorAsync(
        Guid userId,
        string tempToken,
        string code,
        CancellationToken cancellationToken = default);

    Task<AuthTokensResponse> RefreshAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
