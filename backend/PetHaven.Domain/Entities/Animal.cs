using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class Animal : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public AnimalSize Size { get; set; }

    public EnergyLevel EnergyLevel { get; set; }

    public string Description { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public DateTime IntakeDate { get; set; } = DateTime.UtcNow.Date;

    public AnimalStatus Status { get; set; }
        = AnimalStatus.Available;

    public Guid ShelterProfileId { get; set; }

    public ShelterProfile ShelterProfile { get; set; } = null!;
}
