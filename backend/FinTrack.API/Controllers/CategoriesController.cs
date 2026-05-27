using FinTrack.API.Contracts;
using FinTrack.Application.Categories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CreateCategoryCommandHandler _create;
    private readonly GetCategoriesQueryHandler _getAll;
    private readonly GetCategoryByIdQueryHandler _getById;
    private readonly UpdateCategoryCommandHandler _update;
    private readonly DeleteCategoryCommandHandler _delete;
    private readonly IValidator<CreateCategoryCommand> _createValidator;
    private readonly IValidator<UpdateCategoryCommand> _updateValidator;

    public CategoriesController(
        CreateCategoryCommandHandler create,
        GetCategoriesQueryHandler getAll,
        GetCategoryByIdQueryHandler getById,
        UpdateCategoryCommandHandler update,
        DeleteCategoryCommandHandler delete,
        IValidator<CreateCategoryCommand> createValidator,
        IValidator<UpdateCategoryCommand> updateValidator)
    {
        _create = create;
        _getAll = getAll;
        _getById = getById;
        _update = update;
        _delete = delete;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Type, request.Color);
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _create.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _getAll.HandleAsync(new GetCategoriesQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _getById.HandleAsync(new GetCategoryByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Color);
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);
        return Ok(await _update.HandleAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
