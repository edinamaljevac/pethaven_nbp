using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs;

public class AdoptionApplicationDto
{
    public Guid Id { get; set; }
    public Guid AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AnimalSpecies { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public Guid AdopterProfileId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public string AdopterEmail { get; set; } = string.Empty;
    public AdoptionApplicationStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? InterviewScheduledAt { get; set; }
    public DateTime? HomeVisitScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
