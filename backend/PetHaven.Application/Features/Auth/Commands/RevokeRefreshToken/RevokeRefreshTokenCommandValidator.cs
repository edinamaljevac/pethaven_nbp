using FluentValidation;

namespace PetHaven.Application.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}