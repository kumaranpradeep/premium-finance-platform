using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremiumFinance.Application.Common;
using PremiumFinance.Application.Users;

namespace PremiumFinance.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _users;

    public UsersController(IUserAdminService users) => _users = users;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserSummary>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _users.GetUsersAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserSummary), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserSummary>> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        UserSummary user = await _users.CreateUserAsync(request, cancellationToken);
        // 201 Created: a new resource exists. (There's no GET-by-id yet, so no Location header.)
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        await _users.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(string id, CancellationToken cancellationToken)
    {
        await _users.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }
}