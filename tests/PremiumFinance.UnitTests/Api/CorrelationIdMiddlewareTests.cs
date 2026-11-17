using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PremiumFinance.Api.Middleware;

namespace PremiumFinance.UnitTests.Api;

public class CorrelationIdMiddlewareTests
{
    private static async Task<(HttpContext Context, string? SeenByNext)> RunWith(string? incomingHeader)
    {
        var context = new DefaultHttpContext();
        if (incomingHeader is not null)
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = incomingHeader;

        string? seenByNext = null;
        var middleware = new CorrelationIdMiddleware(
            ctx => { seenByNext = ctx.TraceIdentifier; return Task.CompletedTask; },
            NullLogger<CorrelationIdMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        return (context, seenByNext);
    }

    [Fact]
    public async Task Uses_the_callers_id_when_valid()
    {
        var (context, seenByNext) = await RunWith("staff-portal-7f3a9c");

        Assert.Equal("staff-portal-7f3a9c", context.Response.Headers[CorrelationIdMiddleware.HeaderName]);
        Assert.Equal("staff-portal-7f3a9c", seenByNext);   // the rest of the pipeline sees the same ID
    }

    [Fact]
    public async Task Creates_an_id_when_none_is_sent()
    {
        var (context, _) = await RunWith(null);

        string id = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.Matches("^[0-9a-f]{32}$", id);
        Assert.Equal(id, context.TraceIdentifier);
    }

    [Fact]
    public async Task Replaces_an_unsafe_id()
    {
        var (context, _) = await RunWith("abc\r\nFAKE LOG LINE: payment approved");

        string id = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.Matches("^[0-9a-f]{32}$", id);
    }
}