using FinTrack.Domain.Identity;

namespace FinTrack.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
