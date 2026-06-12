namespace PetHaven.Application.DTOs;

public class DonationDto
{
    public Guid Id { get; set; }
    public Guid ShelterProfileId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
}