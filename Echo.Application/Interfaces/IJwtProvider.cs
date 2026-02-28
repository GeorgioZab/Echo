using Echo.Domain;

namespace Echo.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}