namespace PetHaven.Application.DTOs;

public class ShelterDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsVerified { get; set; }
}