using FinTrack.API.Contracts;
using FinTrack.Application.Identity.Login;
using FinTrack.Application.Identity.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterUserCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginQuery(request.Email, request.Password),
            cancellationToken);

        return Ok(result);
    }
}
