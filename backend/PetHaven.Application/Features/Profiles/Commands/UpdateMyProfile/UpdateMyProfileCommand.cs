using MediatR;
using PetHaven.Application.DTOs.Profile;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Profiles.Commands.UpdateMyProfile;

public class UpdateMyProfileCommand : IRequest<MyProfileDto>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? ShelterName { get; set; }
    public string? ShelterLocation { get; set; }
    public string? ShelterContactPhone { get; set; }
    public string? ShelterDescription { get; set; }
    public double? ShelterLatitude { get; set; }
    public double? ShelterLongitude { get; set; }

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