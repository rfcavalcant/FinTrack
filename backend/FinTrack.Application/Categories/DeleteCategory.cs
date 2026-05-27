using FinTrack.Application.Common;
using FinTrack.Application.Common.Exceptions;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Domain.Categories;
using FinTrack.Domain.Common;
using FinTrack.Domain.Transactions;

namespace FinTrack.Application.Categories;

public sealed record DeleteCategoryCommand(Guid Id);

public sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _categories = categories;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task HandleAsync(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null || category.UserId != _currentUser.UserId)
            throw new NotFoundException("Category not found.");

        if (await _transactions.ExistsForCategoryAsync(category.Id, cancellationToken))
            throw new DomainException("Cannot delete a category that has transactions.");

        _categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
