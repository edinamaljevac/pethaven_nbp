using PetHaven.Domain.Entities;

namespace PetHaven.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);

    RefreshToken GenerateRefreshToken(User user);
}