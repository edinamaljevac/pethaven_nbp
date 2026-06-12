using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class FosterReport : BaseEntity
{
    public Guid FosterAssignmentId { get; set; }
    public string BehaviorNotes { get; set; } = string.Empty;
    public string ProgressNotes { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public FosterAssignment FosterAssignment { get; set; } = null!;
}