using MediatR;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Animals.Commands.CreateAnimal;

public class CreateAnimalCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public AnimalSize Size { get; set; }

    public EnergyLevel EnergyLevel { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime IntakeDate { get; set; } = DateTime.UtcNow.Date;

    public Guid ShelterProfileId { get; set; }
}
