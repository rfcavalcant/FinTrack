using FinTrack.API.Contracts;
using FinTrack.Application.Transactions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly RegisterTransactionCommandHandler _register;
    private readonly GetTransactionsQueryHandler _getAll;
    private readonly GetTransactionByIdQueryHandler _getById;
    private readonly DeleteTransactionCommandHandler _delete;
    private readonly IValidator<RegisterTransactionCommand> _registerValidator;

    public TransactionsController(
        RegisterTransactionCommandHandler register,
        GetTransactionsQueryHandler getAll,
        GetTransactionByIdQueryHandler getById,
        DeleteTransactionCommandHandler delete,
        IValidator<RegisterTransactionCommand> registerValidator)
    {
        _register = register;
        _getAll = getAll;
        _getById = getById;
        _delete = delete;
        _registerValidator = registerValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterTransactionRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterTransactionCommand(
            request.AccountId, request.CategoryId, request.Type,
            request.Amount, request.Date, request.Description);
        await _registerValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _register.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? accountId,
        CancellationToken cancellationToken)
        => Ok(await _getAll.HandleAsync(new GetTransactionsQuery(from, to, categoryId, accountId), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _getById.HandleAsync(new GetTransactionByIdQuery(id), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteTransactionCommand(id), cancellationToken);
        return NoContent();
    }
}
