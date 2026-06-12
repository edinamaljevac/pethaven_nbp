using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.Shelters.Commands.CreateShelter;

public class CreateShelterCommandHandler : IRequestHandler<CreateShelterCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateShelterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateShelterCommand request, CancellationToken cancellationToken)
    {
        var shelter = new ShelterProfile
        {
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            Description = request.Description.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        await _unitOfWork.ShelterProfiles.AddAsync(shelter);
        await _unitOfWork.SaveChangesAsync();
        return shelter.Id;
    }
}