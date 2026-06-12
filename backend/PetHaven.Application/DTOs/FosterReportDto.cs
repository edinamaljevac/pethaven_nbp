namespace PetHaven.Application.DTOs;
public class FosterReportDto
{
    public Guid Id { get; set; }
    public Guid FosterAssignmentId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string FosterName { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public string BehaviorNotes { get; set; } = string.Empty;
    public string ProgressNotes { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
