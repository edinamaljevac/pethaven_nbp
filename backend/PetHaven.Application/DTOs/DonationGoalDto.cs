namespace PetHaven.Application.DTOs;
public class DonationGoalDto
{
    public Guid Id { get; set; }
    public Guid ShelterProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public bool IsCompleted { get; set; }
}