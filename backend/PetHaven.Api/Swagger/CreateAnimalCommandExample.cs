using PetHaven.Application.Features.Animals.Commands.CreateAnimal;
using PetHaven.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Swagger;

public class CreateAnimalCommandExample : IExamplesProvider<CreateAnimalCommand>
{
    public CreateAnimalCommand GetExamples() => new()
    {
        Name = "Luna",
        Species = "Dog",
        Breed = "Mixed",
        Age = 3,
        Gender = "Female",
        Size = AnimalSize.Medium,
        EnergyLevel = EnergyLevel.Medium,
        Description = "Friendly dog, good with children.",
        ShelterProfileId = Guid.Parse("11111111-1111-1111-1111-111111111111")
    };
}