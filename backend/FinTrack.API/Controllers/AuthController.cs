using FinTrack.API.Contracts;
using FinTrack.Application.Identity.Login;
using FinTrack.Application.Identity.Register;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserCommandHandler _register;
    private readonly LoginQueryHandler _login;
    private readonly IValidator<RegisterUserCommand> _registerValidator;
    private readonly IValidator<LoginQuery> _loginValidator;

    public AuthController(
        RegisterUserCommandHandler register,
        LoginQueryHandler login,
        IValidator<RegisterUserCommand> registerValidator,
        IValidator<LoginQuery> loginValidator)
    {
        _register = register;
        _login = login;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
        await _registerValidator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _register.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var query = new LoginQuery(request.Email, request.Password);
        await _loginValidator.ValidateAndThrowAsync(query, cancellationToken);
        var result = await _login.HandleAsync(query, cancellationToken);
        return Ok(result);
    }
}
