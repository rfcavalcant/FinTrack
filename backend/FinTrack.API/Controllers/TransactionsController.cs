using FinTrack.API.Contracts;
using FinTrack.Application.Transactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ISender _mediator;

    public TransactionsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Register(RegisterTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterTransactionCommand(
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Amount,
                request.Date,
                request.Description),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? accountId,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetTransactionsQuery(from, to, categoryId, accountId), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetTransactionByIdQuery(id), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTransactionCommand(id), cancellationToken);
        return NoContent();
    }
}
