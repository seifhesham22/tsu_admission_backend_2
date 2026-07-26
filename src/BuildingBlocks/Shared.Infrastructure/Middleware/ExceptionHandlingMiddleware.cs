using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Exceptions;

namespace Shared.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private const int ClientClosedRequest = 499;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (context.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "Unhandled exception after the response started. TraceId {TraceId}",
                traceId);
            return;
        }

        var problem = Map(exception, traceId);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. TraceId {TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);
        }
        else
        {
            _logger.LogWarning(
                "Request failed for {Method} {Path} with status {Status}. TraceId {TraceId}. Reason: {Reason}",
                context.Request.Method,
                context.Request.Path,
                problem.Status,
                traceId,
                exception.Message);
        }

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, SerializerOptions));
    }

    private ProblemDetails Map(Exception exception, string traceId)
    {
        var problem = exception switch
        {
            NotFoundException ex => Create(StatusCodes.Status404NotFound, "Resource not found", ex.Message),
            ForbiddenException ex => Create(StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            ConflictException ex => Create(StatusCodes.Status409Conflict, "Conflict", ex.Message),
            ValidationException ex => CreateValidation(ex),
            DomainRuleException ex => Create(StatusCodes.Status400BadRequest, "Business rule violated", ex.Message),
            UnauthorizedAccessException => Create(StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required."),
            OperationCanceledException => Create(ClientClosedRequest, "Request cancelled", "The request was cancelled."),
            _ => Create(
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                _environment.IsDevelopment()
                    ? exception.ToString()
                    : "An unexpected error occurred. Use the traceId when contacting support.")
        };

        problem.Extensions["traceId"] = traceId;
        return problem;
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = detail
        };

    private static ProblemDetails CreateValidation(ValidationException exception)
    {
        var problem = new ValidationProblemDetails(
            exception.Errors.ToDictionary(x => x.Key, x => x.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };

        return problem;
    }
}
