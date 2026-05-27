using FinTrack.API.Contracts;
using FinTrack.Application.Categories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categories")]
public sealed class CategoriesController(
    CreateCategoryCommandHandler create,
    GetCategoriesQueryHandler getAll,
    GetCategoryByIdQueryHandler getById,
    UpdateCategoryCommandHandler update,
    DeleteCategoryCommandHandler delete,
    IValidator<CreateCategoryCommand> createValidator,
    IValidator<UpdateCategoryCommand> updateValidator)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Type, request.Color);
        await createValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await create.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await getAll.HandleAsync(new GetCategoriesQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await getById.HandleAsync(new GetCategoryByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Color);
        await updateValidator.ValidateAndThrowAsync(command, cancellationToken);
        return Ok(await update.HandleAsync(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await delete.HandleAsync(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
