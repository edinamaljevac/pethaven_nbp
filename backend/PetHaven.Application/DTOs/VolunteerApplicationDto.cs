namespace PetHaven.Application.DTOs;

public class VolunteerApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShelterProfileId { get; set; }
    public string PreferredActivities { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string Status { get; set; } = "Submitted";
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
