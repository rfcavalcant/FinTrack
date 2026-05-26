namespace FinTrack.Application.Identity;

public sealed record AuthenticationResult(Guid UserId, string Name, string Email, string Token);
