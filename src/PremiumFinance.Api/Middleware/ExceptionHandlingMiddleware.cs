using Microsoft.AspNetCore.Mvc;
using PremiumFinance.Application.Exceptions;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            NotFoundException     => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException     => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            _                     => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{ExceptionType} on {Method} {Path}: {Message}",
                ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            // Expected errors carry our own message; unexpected ones never leak internals to the client.
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Something went wrong. Please contact support with the trace ID."
                : ex.Message,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
    }
}