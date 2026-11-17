using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PremiumFinance.Api.Middleware;
using PremiumFinance.Application.Exceptions;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.UnitTests.Api;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int Status, string ContentType, ProblemDetails Problem)> RunWith(Exception thrown)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/agreements/PCL-001/payments";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw thrown,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return (context.Response.StatusCode, context.Response.ContentType!, problem!);
    }

    [Fact]
    public async Task NotFound_becomes_404_with_the_message()
    {
        var (status, _, problem) = await RunWith(new NotFoundException("Agreement PCL-001 was not found."));

        Assert.Equal(404, status);
        Assert.Equal("Agreement PCL-001 was not found.", problem.Detail);
    }

    [Fact]
    public async Task Broken_business_rule_becomes_422()
    {
        var (status, _, problem) = await RunWith(
            new BusinessRuleException("Agreement PCL-001 is Cancelled; payments can't be recorded."));

        Assert.Equal(422, status);
        Assert.Equal("Business rule violated", problem.Title);
    }

    [Fact]
    public async Task Conflict_becomes_409()
    {
        var (status, _, _) = await RunWith(new ConflictException("Agreement PCL-001 was changed by someone else."));

        Assert.Equal(409, status);
    }

    [Fact]
    public async Task Unexpected_error_becomes_500_without_leaking_details()
    {
        var (status, contentType, problem) = await RunWith(
            new InvalidOperationException("Connection string 'Server=prod-sql;Password=...' failed"));

        Assert.Equal(500, status);
        Assert.Equal("application/problem+json", contentType);
        Assert.DoesNotContain("Password", problem.Detail);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }
}