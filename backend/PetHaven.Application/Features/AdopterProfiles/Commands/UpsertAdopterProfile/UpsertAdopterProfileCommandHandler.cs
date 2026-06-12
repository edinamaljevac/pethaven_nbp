using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.AdopterProfiles.Commands.UpsertAdopterProfile;

public class UpsertAdopterProfileCommandHandler : IRequestHandler<UpsertAdopterProfileCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpsertAdopterProfileCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(UpsertAdopterProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.AdopterProfiles.FirstOrDefaultAsync(x => x.UserId == request.UserId);
        if (profile is null)
        {
            profile = new AdopterProfile { UserId = request.UserId };
            await _unitOfWork.AdopterProfiles.AddAsync(profile);
        }
        profile.Address = request.Address.Trim(); profile.HousingType = request.HousingType; profile.HouseholdMembers = request.HouseholdMembers; profile.HasChildren = request.HasChildren; profile.HasOtherPets = request.HasOtherPets; profile.ExperienceWithPets = request.ExperienceWithPets.Trim(); profile.AdoptionReason = request.AdoptionReason.Trim(); profile.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return profile.Id;
    }
}