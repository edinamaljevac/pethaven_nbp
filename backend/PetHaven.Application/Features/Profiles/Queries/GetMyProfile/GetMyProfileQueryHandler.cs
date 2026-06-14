using MediatR;
using PetHaven.Application.DTOs.Profile;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Profiles.Queries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found.");
        var dto = new MyProfileDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };

        if (user.Role == UserRole.Shelter)
        {
            var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile is not null)
            {
                dto.ProfileId = profile.Id;
                dto.ShelterName = profile.Name;
                dto.ShelterLocation = profile.Location;
                dto.ShelterContactPhone = profile.ContactPhone;
                dto.ShelterDescription = profile.Description;
                dto.ShelterLatitude = profile.Latitude;
                dto.ShelterLongitude = profile.Longitude;
                dto.ShelterIsVerified = profile.IsVerified;
            }
        }

        if (user.Role == UserRole.Adopter)
        {
            var profile = await _unitOfWork.AdopterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile is not null)
            {
                dto.ProfileId = profile.Id;
                dto.AdopterAddress = profile.Address;
                dto.AdopterCountry = profile.Country;
                dto.AdopterHousingType = profile.HousingType;
                dto.AdopterHouseholdMembers = profile.HouseholdMembers;
                dto.AdopterHasChildren = profile.HasChildren;
                dto.AdopterHasOtherPets = profile.HasOtherPets;
                dto.AdopterExperienceWithPets = profile.ExperienceWithPets;
                dto.AdopterAdoptionReason = profile.AdoptionReason;
            }
        }

        if (user.Role == UserRole.Foster)
        {
            var profile = await _unitOfWork.FosterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (profile is not null)
            {
                dto.ProfileId = profile.Id;
                dto.FosterPreferredAnimalType = profile.PreferredAnimalType;
                dto.FosterCapacity = profile.Capacity;
                dto.FosterAvailableFrom = profile.AvailableFrom;
                dto.FosterAvailableTo = profile.AvailableTo;
            }
        }

        return dto;
    }
}
