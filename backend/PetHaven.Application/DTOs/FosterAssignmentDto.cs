namespace PetHaven.Application.DTOs;
public class FosterAssignmentDto
{
    public Guid Id { get; set; }
    public Guid AnimalId { get; set; }
    public Guid FosterProfileId { get; set; }
    public Guid ShelterProfileId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalSpecies { get; set; } = string.Empty;
    public string FosterName { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
