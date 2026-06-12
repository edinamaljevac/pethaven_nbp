using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Animals.Commands.SetAnimalVideo;

public class SetAnimalVideoCommandHandler : IRequestHandler<SetAnimalVideoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetAnimalVideoCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(SetAnimalVideoCommand request, CancellationToken cancellationToken)
    {
        var animal = await _unitOfWork.Animals.GetByIdAsync(request.AnimalId)
            ?? throw new KeyNotFoundException("Animal not found.");

        animal.VideoUrl = request.VideoUrl.Trim();
        animal.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Animals.Update(animal);
        await _unitOfWork.SaveChangesAsync();
    }
}
