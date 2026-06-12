using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs;

public class AnimalDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public AnimalSize Size { get; set; }

    public EnergyLevel EnergyLevel { get; set; }

    public string Description { get; set; } = string.Empty;

    public string VideoUrl { get; set; } = string.Empty;

    public DateTime IntakeDate { get; set; }

    public int DaysInShelter { get; set; }

    public AnimalStatus Status { get; set; }

    public Guid ShelterProfileId { get; set; }

    public string ShelterName { get; set; } = string.Empty;

    public HealthRecordDto? HealthRecord { get; set; }

    public BehaviorProfileDto? BehaviorProfile { get; set; }

    public List<MediaFileDto> Photos { get; set; } = [];

    public bool IsSpecialNeedsSpotlight { get; set; }

    public List<string> SpecialNeedsReasons { get; set; } = [];
}
