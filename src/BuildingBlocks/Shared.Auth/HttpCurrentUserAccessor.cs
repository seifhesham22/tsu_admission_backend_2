using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Kernel.Exceptions;

namespace Shared.Auth;

public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser Get()
    {
        if (!TryGet(out var user) || user is null)
        {
            throw new ForbiddenException("The request is not associated with an authenticated user.");
        }

        return user;
    }

    public bool TryGet(out CurrentUser? user)
    {
        user = null;

        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var idValue =
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("sub");

        if (!Guid.TryParse(idValue, out var id))
        {
            return false;
        }

        var role = principal.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        user = new CurrentUser
        {
            Id = id,
            Role = role,
            Email = principal.FindFirstValue(ClaimTypes.Email)
        };

        return true;
    }
}
