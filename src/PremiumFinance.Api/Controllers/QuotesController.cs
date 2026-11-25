using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PremiumFinance.Application.Common;
using PremiumFinance.Application.Quotes;

namespace PremiumFinance.Api.Controllers;

[ApiController]
[Route("api/quotes")]
[Authorize(Roles = $"{Roles.Broker},{Roles.Agent},{Roles.TeamLeader}")]
public class QuotesController : ControllerBase
{
    private readonly QuoteService _quotes;

    public QuotesController(QuoteService quotes) => _quotes = quotes;

    [HttpPost]
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteResponse>> GetQuote(QuoteRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _quotes.GetQuoteAsync(request, cancellationToken));
    }
}