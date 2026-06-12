using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Shelters.Commands.VerifyShelter;

public class VerifyShelterCommandHandler : IRequestHandler<VerifyShelterCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public VerifyShelterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(VerifyShelterCommand request, CancellationToken cancellationToken)
    {
        var shelter = await _unitOfWork.ShelterProfiles.GetByIdAsync(request.ShelterId) ?? throw new KeyNotFoundException("Shelter not found.");
        shelter.IsVerified = request.IsVerified;
        shelter.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ShelterProfiles.Update(shelter);
        await _unitOfWork.SaveChangesAsync();
    }
}