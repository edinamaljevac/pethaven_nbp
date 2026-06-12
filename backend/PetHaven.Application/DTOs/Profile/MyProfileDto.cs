using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs.Profile;

public class MyProfileDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public Guid? ProfileId { get; set; }

    public string? ShelterName { get; set; }
    public string? ShelterLocation { get; set; }
    public string? ShelterContactPhone { get; set; }
    public string? ShelterDescription { get; set; }
    public double? ShelterLatitude { get; set; }
    public double? ShelterLongitude { get; set; }
    public bool? ShelterIsVerified { get; set; }

    public string? AdopterAddress { get; set; }
    public HousingType? AdopterHousingType { get; set; }
    public int? AdopterHouseholdMembers { get; set; }
    public bool? AdopterHasChildren { get; set; }
    public bool? AdopterHasOtherPets { get; set; }
    public string? AdopterExperienceWithPets { get; set; }
    public string? AdopterAdoptionReason { get; set; }

    public string? FosterPreferredAnimalType { get; set; }
    public int? FosterCapacity { get; set; }
    public DateTime? FosterAvailableFrom { get; set; }
    public DateTime? FosterAvailableTo { get; set; }
}