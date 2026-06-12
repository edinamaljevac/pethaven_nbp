using MediatR;
using PetHaven.Application.Interfaces;
namespace PetHaven.Application.Features.Admin.Commands.ModerateAnimal;
public class ModerateAnimalCommandHandler : IRequestHandler<ModerateAnimalCommand>
{
    private readonly IUnitOfWork _uow;
    public ModerateAnimalCommandHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task Handle(ModerateAnimalCommand r, CancellationToken ct)
    {
        var animal=await _uow.Animals.GetByIdAsync(r.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        animal.Status=r.Status; animal.UpdatedAt=DateTime.UtcNow; _uow.Animals.Update(animal); await _uow.SaveChangesAsync();
    }
}