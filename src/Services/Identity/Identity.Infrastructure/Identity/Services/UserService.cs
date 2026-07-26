using Contracts.IntegrationEvents;
using Identity.Application.Users.Contracts;
using Identity.Application.Users.Dtos;
using Identity.Application.Users;
using Identity.Infrastructure.Identity.Contracts;
using Identity.Infrastructure.Identity.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Auth;
using Shared.Kernel.Exceptions;
using System.Security.Cryptography;

namespace Identity.Infrastructure.Identity.Services;

public sealed class UserService : IUserService
{
    private const string EmailSubject = "Admission authentication service";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokens;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokens,
        IPublishEndpoint publishEndpoint,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _tokens = tokens;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new ConflictException($"The email '{request.Email}' is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            TwoFactorEnabled = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        EnsureSucceeded(result, "Unable to create the user account.");

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Applicant);
        EnsureSucceeded(roleResult, "Unable to assign the applicant role.");

        await _publishEndpoint.Publish(
            new UserRegistered
            {
                UserId = user.Id,
                Email = request.Email,
                UserName = request.UserName,
                Role = Roles.Applicant,
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task<TwoFactorChallengeResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new ForbiddenException("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new ForbiddenException("This account is locked out.");
        }

        var tempToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTime.UtcNow.AddMinutes(3);

        user.TempToken = tempToken;
        user.TempTokenExpiresAtUtc = expiresAt;
        EnsureSucceeded(await _userManager.UpdateAsync(user), "Unable to start the login challenge.");

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

        await _publishEndpoint.Publish(
            new SendEmailNotification
            {
                To = user.Email!,
                Subject = EmailSubject,
                Body = $"Your verification code is {code}. It expires in 3 minutes.",
                OccurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        return new TwoFactorChallengeResponse(tempToken, expiresAt);
    }

    public async Task<AuthTokensResponse> VerifyTwoFactorAsync(
        Guid userId,
        string tempToken,
        string code,
        CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(userId);

        if (!user.HasValidTempToken(tempToken))
        {
            throw new ForbiddenException("The login challenge is invalid or has expired.");
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultPhoneProvider,
            code);

        if (!isValid)
        {
            throw new ForbiddenException("The verification code is invalid.");
        }

        user.ClearTempToken();
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthTokensResponse> RefreshAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(userId);

        var hash = _tokens.Hash(refreshToken);
        if (user.RefreshTokenHash is null ||
            !CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(user.RefreshTokenHash),
                Convert.FromBase64String(hash)) ||
            user.RefreshTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ForbiddenException("The refresh token is invalid or has expired.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindAsync(userId);

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        EnsureSucceeded(result, "Unable to change the password.");

        user.ClearRefreshToken();
        EnsureSucceeded(await _userManager.UpdateAsync(user), "Unable to revoke existing sessions.");
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(userId);

        user.ClearRefreshToken();
        user.ClearTempToken();

        EnsureSucceeded(await _userManager.UpdateAsync(user), "Unable to complete the logout.");
    }

    private async Task<AuthTokensResponse> IssueTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, accessExpiresAt) = _tokens.CreateAccessToken(user, roles);
        var (refreshToken, refreshHash, refreshExpiresAt) = _tokens.CreateRefreshToken();

        user.RefreshTokenHash = refreshHash;
        user.RefreshTokenExpiresAtUtc = refreshExpiresAt;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Failed to persist refresh token for user {UserId}: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(x => x.Description)));

            throw new DomainRuleException("Unable to complete authentication.");
        }

        return new AuthTokensResponse(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    private async Task<ApplicationUser> FindAsync(Guid userId) =>
        await _userManager.FindByIdAsync(userId.ToString())
        ?? throw new NotFoundException("The user account was not found.");

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join(" ", result.Errors.Select(x => x.Description));
        throw new DomainRuleException($"{message} {details}".Trim());
    }
}
