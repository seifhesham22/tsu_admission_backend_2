namespace Identity.Application.Users.Dtos;

public sealed record AuthTokensResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
