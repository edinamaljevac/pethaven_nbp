using MediatR;

namespace PetHaven.Application.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommand : IRequest<Unit>
{
    public string RefreshToken { get; set; } = string.Empty;
}