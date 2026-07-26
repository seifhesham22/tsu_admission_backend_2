using Microsoft.AspNetCore.Builder;

namespace Shared.Infrastructure.Middleware;

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseSharedExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
