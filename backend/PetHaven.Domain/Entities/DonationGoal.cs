using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class DonationGoal : BaseEntity
{
    public Guid ShelterProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public bool IsCompleted { get; set; }

    public ShelterProfile ShelterProfile { get; set; } = null!;
}