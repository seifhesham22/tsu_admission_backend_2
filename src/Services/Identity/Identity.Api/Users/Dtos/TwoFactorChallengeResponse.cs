namespace Identity.Api.Users.Dtos;

public sealed record TwoFactorChallengeResponse(string TempToken, DateTime ExpiresAtUtc);
