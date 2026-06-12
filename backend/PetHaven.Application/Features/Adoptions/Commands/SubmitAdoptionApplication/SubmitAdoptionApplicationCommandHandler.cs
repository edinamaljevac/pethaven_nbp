using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Commands.SubmitAdoptionApplication;

public class SubmitAdoptionApplicationCommandHandler : IRequestHandler<SubmitAdoptionApplicationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public SubmitAdoptionApplicationCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(SubmitAdoptionApplicationCommand request, CancellationToken cancellationToken)
    {
        var animal = await _unitOfWork.Animals.GetByIdAsync(request.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        if (animal.Status != AnimalStatus.Available) throw new InvalidOperationException("Animal is not available for adoption.");
        var application = new AdoptionApplication { AnimalId = request.AnimalId, AdopterProfileId = request.AdopterProfileId, Notes = request.Notes.Trim(), Status = AdoptionApplicationStatus.Submitted };
        animal.Status = AnimalStatus.InAdoptionProcess;
        await _unitOfWork.AdoptionApplications.AddAsync(application);
        _unitOfWork.Animals.Update(animal);
        await _unitOfWork.SaveChangesAsync();
        return application.Id;
    }
}