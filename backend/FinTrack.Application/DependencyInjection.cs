using System.Reflection;
using FinTrack.Application.Accounts;
using FinTrack.Application.Budgeting;
using FinTrack.Application.Categories;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Identity.Login;
using FinTrack.Application.Identity.Register;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Transactions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Identity
        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginQueryHandler>();

        // Accounts
        services.AddScoped<OpenAccountCommandHandler>();
        services.AddScoped<GetAccountsQueryHandler>();
        services.AddScoped<GetAccountByIdQueryHandler>();
        services.AddScoped<RenameAccountCommandHandler>();
        services.AddScoped<DeleteAccountCommandHandler>();

        // Categories
        services.AddScoped<CreateCategoryCommandHandler>();
        services.AddScoped<GetCategoriesQueryHandler>();
        services.AddScoped<GetCategoryByIdQueryHandler>();
        services.AddScoped<UpdateCategoryCommandHandler>();
        services.AddScoped<DeleteCategoryCommandHandler>();

        // Transactions
        services.AddScoped<RegisterTransactionCommandHandler>();
        services.AddScoped<GetTransactionsQueryHandler>();
        services.AddScoped<GetTransactionByIdQueryHandler>();
        services.AddScoped<DeleteTransactionCommandHandler>();

        // Budgeting
        services.AddScoped<DefineBudgetCommandHandler>();
        services.AddScoped<GetBudgetsQueryHandler>();
        services.AddScoped<GetBudgetByIdQueryHandler>();
        services.AddScoped<DeleteBudgetCommandHandler>();

        // Domain event handlers
        services.AddScoped<IDomainEventHandler<TransactionRegistered>, OnTransactionRegisteredHandler>();

        return services;
    }
}
