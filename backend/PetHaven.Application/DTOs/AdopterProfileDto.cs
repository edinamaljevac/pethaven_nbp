using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs;

public class AdopterProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public HousingType HousingType { get; set; }
    public int HouseholdMembers { get; set; }
    public bool HasChildren { get; set; }
    public bool HasOtherPets { get; set; }
    public string ExperienceWithPets { get; set; } = string.Empty;
    public string AdoptionReason { get; set; } = string.Empty;
}