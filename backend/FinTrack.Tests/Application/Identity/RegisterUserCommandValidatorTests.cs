using FinTrack.Application.Identity.Register;
using FluentValidation.TestHelper;

namespace FinTrack.Tests.Application.Identity;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Nome_Vazio_FalhaValidacao()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("", "rafael@fintrack.com", "senhaForte123"));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Email_Invalido_FalhaValidacao()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("Rafael", "nao-e-email", "senhaForte123"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Senha_MuitoCurta_FalhaValidacao()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("Rafael", "rafael@fintrack.com", "123"));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Comando_Valido_PassaValidacao()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("Rafael", "rafael@fintrack.com", "senhaForte123"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
