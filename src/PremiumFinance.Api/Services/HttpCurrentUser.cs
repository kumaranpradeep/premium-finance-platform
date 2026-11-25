using System.Security.Claims;
using PremiumFinance.Application.Common;

namespace PremiumFinance.Api.Services;

public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    // The signed-in user's ID from their token; "system" when there's no user (background jobs, startup).
    // The signed-in user's ID ("sub" claim in the JWT); "system" when there's no user (background jobs, startup).
    public string UserId =>
        _accessor.HttpContext?.User.FindFirstValue("sub") ?? "system";
}