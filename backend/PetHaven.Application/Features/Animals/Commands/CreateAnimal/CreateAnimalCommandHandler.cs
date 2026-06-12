using MediatR;
using PetHaven.Application.Features.Notifications.Events;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.Animals.Commands.CreateAnimal;

public class CreateAnimalCommandHandler : IRequestHandler<CreateAnimalCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateAnimalCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(
        CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        var animal = new Animal
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            Age = request.Age,
            Gender = request.Gender,
            Size = request.Size,
            EnergyLevel = request.EnergyLevel,
            Description = request.Description,
            IntakeDate = DateTime.SpecifyKind(request.IntakeDate.Date, DateTimeKind.Utc),
            ShelterProfileId = request.ShelterProfileId
        };

        await _unitOfWork.Animals.AddAsync(animal);
        await _unitOfWork.SaveChangesAsync();
        await _mediator.Publish(new AnimalBecameAvailableNotification(animal.Id), cancellationToken);

        return animal.Id;
    }
}
