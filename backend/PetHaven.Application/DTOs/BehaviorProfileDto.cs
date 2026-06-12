using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs;

public class BehaviorProfileDto
{
    public Guid Id { get; set; }
    public Guid AnimalId { get; set; }
    public EnergyLevel EnergyLevel { get; set; }
    public bool GoodWithChildren { get; set; }
    public bool GoodWithDogs { get; set; }
    public bool GoodWithCats { get; set; }
    public bool HasSpecialNeeds { get; set; }
    public string BehaviorDescription { get; set; } = string.Empty;
}