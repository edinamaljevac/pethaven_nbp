using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.Fosters.Commands.CreateFosterProfile;

public class CreateFosterProfileCommandHandler : IRequestHandler<CreateFosterProfileCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateFosterProfileCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<Guid> Handle(CreateFosterProfileCommand request, CancellationToken cancellationToken)
    {
        if (request.Capacity <= 0) throw new InvalidOperationException("Capacity must be greater than 0.");
        if (request.AvailableFrom >= request.AvailableTo) throw new InvalidOperationException("Available From must be before Available To.");

        var profile = await _unitOfWork.FosterProfiles.FirstOrDefaultAsync(x => x.UserId == request.UserId);
        if (profile is null)
        {
            profile = new FosterProfile { UserId = request.UserId };
            await _unitOfWork.FosterProfiles.AddAsync(profile);
        }

        profile.PreferredAnimalType = request.PreferredAnimalType.Trim();
        profile.Capacity = request.Capacity;
        profile.AvailableFrom = request.AvailableFrom;
        profile.AvailableTo = request.AvailableTo;
        profile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.FosterProfiles.Update(profile);
        await _unitOfWork.SaveChangesAsync();
        return profile.Id;
    }
}
