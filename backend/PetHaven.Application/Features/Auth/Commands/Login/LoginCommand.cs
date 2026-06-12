using MediatR;
using PetHaven.Application.DTOs.Auth;

namespace PetHaven.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}