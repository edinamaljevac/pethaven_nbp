using MediatR;
using PetHaven.Application.DTOs.Auth;

namespace PetHaven.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}