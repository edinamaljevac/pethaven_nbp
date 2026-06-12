using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class BehaviorProfile : BaseEntity
{
    public Guid AnimalId { get; set; }

    public EnergyLevel EnergyLevel { get; set; }

    public bool GoodWithChildren { get; set; }

    public bool GoodWithDogs { get; set; }

    public bool GoodWithCats { get; set; }

    public bool HasSpecialNeeds { get; set; }

    public string BehaviorDescription { get; set; } = string.Empty;

    public Animal Animal { get; set; } = null!;
}