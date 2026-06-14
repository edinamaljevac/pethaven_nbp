using MediatR;
using PetHaven.Application.DTOs.Profile;
using PetHaven.Application.Features.Profiles.Queries.GetMyProfile;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Profiles.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, MyProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyProfileCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MyProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId) ?? throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Shelter)
        {
            user.FirstName = request.ShelterName?.Trim() ?? user.FirstName;
            user.LastName = "Shelter";
            await UpsertShelterProfile(user, request);
        }
        else
        {
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();

            if (user.Role == UserRole.Adopter)
            {
                await UpsertAdopterProfile(user, request);
            }

            if (user.Role == UserRole.Foster)
            {
                await UpsertFosterProfile(user, request);
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return await new GetMyProfileQueryHandler(_unitOfWork).Handle(new GetMyProfileQuery { UserId = user.Id }, cancellationToken);
    }

    private async Task UpsertShelterProfile(User user, UpdateMyProfileCommand request)
    {
        var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null)
        {
            profile = new ShelterProfile { UserId = user.Id };
            await _unitOfWork.ShelterProfiles.AddAsync(profile);
        }

        profile.Name = request.ShelterName?.Trim() ?? profile.Name;
        profile.Location = request.ShelterLocation?.Trim() ?? string.Empty;
        profile.ContactPhone = request.ShelterContactPhone?.Trim() ?? string.Empty;
        profile.Description = request.ShelterDescription?.Trim() ?? string.Empty;
        profile.Latitude = request.ShelterLatitude;
        profile.Longitude = request.ShelterLongitude;
        profile.UpdatedAt = DateTime.UtcNow;
    }

    private async Task UpsertAdopterProfile(User user, UpdateMyProfileCommand request)
    {
        var profile = await _unitOfWork.AdopterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null)
        {
            profile = new AdopterProfile { UserId = user.Id };
            await _unitOfWork.AdopterProfiles.AddAsync(profile);
        }

        profile.Address = request.AdopterAddress?.Trim() ?? string.Empty;
        profile.Country = request.AdopterCountry?.Trim() ?? string.Empty;
        profile.HousingType = request.AdopterHousingType ?? HousingType.Apartment;
        profile.HouseholdMembers = request.AdopterHouseholdMembers ?? 1;
        profile.HasChildren = request.AdopterHasChildren ?? false;
        profile.HasOtherPets = request.AdopterHasOtherPets ?? false;
        profile.ExperienceWithPets = request.AdopterExperienceWithPets?.Trim() ?? string.Empty;
        profile.AdoptionReason = request.AdopterAdoptionReason?.Trim() ?? string.Empty;
        profile.UpdatedAt = DateTime.UtcNow;
    }

    private async Task UpsertFosterProfile(User user, UpdateMyProfileCommand request)
    {
        var profile = await _unitOfWork.FosterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null)
        {
            profile = new FosterProfile { UserId = user.Id };
            await _unitOfWork.FosterProfiles.AddAsync(profile);
        }

        profile.PreferredAnimalType = request.FosterPreferredAnimalType?.Trim() ?? string.Empty;
        profile.Capacity = request.FosterCapacity ?? 1;
        profile.AvailableFrom = request.FosterAvailableFrom ?? DateTime.UtcNow;
        profile.AvailableTo = request.FosterAvailableTo ?? DateTime.UtcNow.AddMonths(1);
        profile.UpdatedAt = DateTime.UtcNow;
    }
}
