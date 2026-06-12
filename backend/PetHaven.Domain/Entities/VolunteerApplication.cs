using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class VolunteerApplication : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ShelterProfileId { get; set; }

    public string PreferredActivities { get; set; } = string.Empty;

    public string Availability { get; set; } = string.Empty;

    public string Motivation { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public VolunteerApplicationStatus Status { get; set; } = VolunteerApplicationStatus.Submitted;

    public User User { get; set; } = null!;

    public ShelterProfile ShelterProfile { get; set; } = null!;
}
