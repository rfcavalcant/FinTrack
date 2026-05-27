namespace FinTrack.Application.Identity.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password);
