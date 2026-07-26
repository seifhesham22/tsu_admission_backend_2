namespace Identity.Application.Users.Dtos;

public sealed record TwoFactorChallengeResponse(string TempToken, DateTime ExpiresAtUtc);
