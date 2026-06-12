using FluentValidation;

namespace PetHaven.Application.Features.Animals.Commands.CreateAnimal;

public class CreateAnimalCommandValidator : AbstractValidator<CreateAnimalCommand>
{
    public CreateAnimalCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Species)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Breed)
            .MaximumLength(100);

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Gender)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.IntakeDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Intake date cannot be in the future.");

        RuleFor(x => x.ShelterProfileId)
            .NotEmpty();
    }
}
