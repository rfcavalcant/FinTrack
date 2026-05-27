using FinTrack.API.Contracts;
using FinTrack.Application.Accounts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly OpenAccountCommandHandler _open;
    private readonly GetAccountsQueryHandler _getAll;
    private readonly GetAccountByIdQueryHandler _getById;
    private readonly RenameAccountCommandHandler _rename;
    private readonly DeleteAccountCommandHandler _delete;
    private readonly IValidator<OpenAccountCommand> _openValidator;
    private readonly IValidator<RenameAccountCommand> _renameValidator;

    public AccountsController(
        OpenAccountCommandHandler open,
        GetAccountsQueryHandler getAll,
        GetAccountByIdQueryHandler getById,
        RenameAccountCommandHandler rename,
        DeleteAccountCommandHandler delete,
        IValidator<OpenAccountCommand> openValidator,
        IValidator<RenameAccountCommand> renameValidator)
    {
        _open = open;
        _getAll = getAll;
        _getById = getById;
        _rename = rename;
        _delete = delete;
        _openValidator = openValidator;
        _renameValidator = renameValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Open(OpenAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new OpenAccountCommand(request.Name, request.Type, request.InitialBalance, request.Currency, request.CreditLimit);
        await _openValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _open.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _getAll.HandleAsync(new GetAccountsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _getById.HandleAsync(new GetAccountByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Rename(Guid id, RenameAccountRequest request, CancellationToken cancellationToken)
    {
        var command = new RenameAccountCommand(id, request.Name);
        await _renameValidator.ValidateAndThrowAsync(command, cancellationToken);
        return Ok(await _rename.HandleAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteAccountCommand(id), cancellationToken);
        return NoContent();
    }
}
