using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class FosterAssignment : BaseEntity
{
    public Guid AnimalId { get; set; }
    public Guid FosterProfileId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
    public Animal Animal { get; set; } = null!;
    public FosterProfile FosterProfile { get; set; } = null!;
}