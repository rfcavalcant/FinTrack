using FinTrack.API.Contracts;
using FinTrack.Application.Budgeting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/budgets")]
public sealed class BudgetsController(
    DefineBudgetCommandHandler define,
    GetBudgetsQueryHandler getAll,
    GetBudgetByIdQueryHandler getById,
    DeleteBudgetCommandHandler delete,
    IValidator<DefineBudgetCommand> defineValidator)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Define(DefineBudgetRequest request, CancellationToken cancellationToken)
    {
        var command = new DefineBudgetCommand(
            request.CategoryId, request.Year, request.Month,
            request.LimitAmount, request.Currency);
        await defineValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await define.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year, [FromQuery] int? month,
        CancellationToken cancellationToken)
        => Ok(await getAll.HandleAsync(new GetBudgetsQuery(year, month), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await getById.HandleAsync(new GetBudgetByIdQuery(id), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await delete.HandleAsync(new DeleteBudgetCommand(id), cancellationToken);
        return NoContent();
    }
}
