using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class AdoptionApplication : BaseEntity
{
    public Guid AnimalId { get; set; }

    public Guid AdopterProfileId { get; set; }

    public AdoptionApplicationStatus Status { get; set; }
        = AdoptionApplicationStatus.Submitted;

    public string Notes { get; set; } = string.Empty;
    public DateTime? InterviewScheduledAt { get; set; }
    public DateTime? HomeVisitScheduledAt { get; set; }

    public Animal Animal { get; set; } = null!;

    public AdopterProfile AdopterProfile { get; set; } = null!;
}
