using FluentValidation;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.Role).IsInEnum().NotEqual(UserRole.Admin);

        When(x => x.Role is UserRole.Adopter or UserRole.Foster, () =>
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        });

        When(x => x.Role == UserRole.Shelter, () =>
        {
            RuleFor(x => x.ShelterName).NotEmpty().MaximumLength(150);
        });
    }
}