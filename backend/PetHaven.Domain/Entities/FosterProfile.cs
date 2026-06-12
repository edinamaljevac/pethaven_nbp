using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class FosterProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public string PreferredAnimalType { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public DateTime AvailableFrom { get; set; }

    public DateTime AvailableTo { get; set; }

    public User User { get; set; } = null!;
}