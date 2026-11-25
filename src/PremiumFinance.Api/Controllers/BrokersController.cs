using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremiumFinance.Application.Brokers;
using PremiumFinance.Application.Common;

namespace PremiumFinance.Api.Controllers;

[ApiController]
[Route("api/brokers")]
// Reading: Operations, Compliance, and Admin (who links broker users to their firm).
[Authorize(Roles = $"{Roles.Operations},{Roles.Compliance},{Roles.Admin}")]
public class BrokersController : ControllerBase
{
    private readonly BrokerService _brokers;

    public BrokersController(BrokerService brokers) => _brokers = brokers;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrokerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrokerResponse>>> GetBrokers(CancellationToken cancellationToken)
    {
        return Ok(await _brokers.GetBrokersAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BrokerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerResponse>> GetBroker(int id, CancellationToken cancellationToken)
    {
        return Ok(await _brokers.GetBrokerAsync(id, cancellationToken));
    }

    // Changing: Operations only. Together with the class attribute, both must pass,
    // and Operations is the only role in both lists.
    [HttpPost]
    [Authorize(Roles = Roles.Operations)]
    [ProducesResponseType(typeof(BrokerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BrokerResponse>> CreateBroker(CreateBrokerRequest request, CancellationToken cancellationToken)
    {
        BrokerResponse broker = await _brokers.CreateBrokerAsync(request, cancellationToken);
        // 201 with a Location header pointing at the new broker: /api/brokers/{id}
        return CreatedAtAction(nameof(GetBroker), new { id = broker.Id }, broker);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Operations)]
    [ProducesResponseType(typeof(BrokerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BrokerResponse>> UpdateBroker(int id, UpdateBrokerRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _brokers.UpdateBrokerAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/suspend")]
    [Authorize(Roles = Roles.Operations)]
    [ProducesResponseType(typeof(BrokerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BrokerResponse>> SuspendBroker(int id, SuspendBrokerRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _brokers.SuspendBrokerAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/reinstate")]
    [Authorize(Roles = Roles.Operations)]
    [ProducesResponseType(typeof(BrokerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BrokerResponse>> ReinstateBroker(int id, RowVersionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _brokers.ReinstateBrokerAsync(id, request, cancellationToken));
    }
}