using FinTrack.API.Contracts;
using FinTrack.Application.Budgeting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly ISender _mediator;

    public BudgetsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Define(
        DefineBudgetRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DefineBudgetCommand(
                request.CategoryId, request.Year, request.Month,
                request.LimitAmount, request.Currency),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year, [FromQuery] int? month,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetBudgetsQuery(year, month), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetBudgetByIdQuery(id), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBudgetCommand(id), cancellationToken);
        return NoContent();
    }
}
