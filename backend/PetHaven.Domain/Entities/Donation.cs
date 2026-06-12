using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class Donation : BaseEntity
{
    public Guid ShelterProfileId { get; set; }

    public Guid? UserId { get; set; }

    public decimal Amount { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public bool IsPaid { get; set; }

    public ShelterProfile ShelterProfile { get; set; } = null!;

    public User? User { get; set; }
}