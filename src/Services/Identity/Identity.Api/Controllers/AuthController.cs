using Identity.Application.Users.Contracts;
using Identity.Application.Users.Dtos;
using Identity.Application.Users;
using Identity.Infrastructure.Identity.Authentication;
using Identity.Infrastructure.Identity.Contracts;
using Identity.Infrastructure.Identity.Models;
using Identity.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Kernel.Exceptions;
using System.Security.Claims;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserService _users;

    public AuthController(IUserService users)
    {
        _users = users;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        await _users.RegisterAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TwoFactorChallengeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TwoFactorChallengeResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _users.LoginAsync(request, cancellationToken));

    [HttpPost("verify")]
    [Authorize(AuthenticationSchemes = TempTokenAuthenticationHandler.SchemeName)]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokensResponse>> Verify(
        [FromBody] VerifyTwoFactorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tempToken = Request.Headers["X-Temp-Token"].ToString();
        var tokens = await _users.VerifyTwoFactorAsync(
            GetUserId(),
            tempToken,
            request.Code,
            cancellationToken);

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(AuthTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokensResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Ok(await _users.RefreshAsync(GetUserId(), request.RefreshToken, cancellationToken));
    }

    [HttpPost("change-password")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.AnyAuthenticated)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _users.ChangePasswordAsync(GetUserId(), request, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.AnyAuthenticated)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _users.LogoutAsync(GetUserId(), cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new ForbiddenException("The token does not contain a valid user identifier.");
    }
}
