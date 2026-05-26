using System.Security.Claims;
using FinTrack.Application.Common.Interfaces;

namespace FinTrack.API;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var sub = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user in the current context.");
        }
    }
}
