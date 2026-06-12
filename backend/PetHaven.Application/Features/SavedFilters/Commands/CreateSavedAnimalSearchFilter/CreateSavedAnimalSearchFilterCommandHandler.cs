using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.SavedFilters.Commands.CreateSavedAnimalSearchFilter;
public class CreateSavedAnimalSearchFilterCommandHandler : IRequestHandler<CreateSavedAnimalSearchFilterCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    public CreateSavedAnimalSearchFilterCommandHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task<Guid> Handle(CreateSavedAnimalSearchFilterCommand r, CancellationToken ct)
    {
        var filter=new SavedAnimalSearchFilter{UserId=r.UserId,Name=r.Name.Trim(),Species=r.Species,Breed=r.Breed,Size=r.Size,EnergyLevel=r.EnergyLevel,SpecialNeedsOnly=r.SpecialNeedsOnly,City=r.City,Latitude=r.Latitude,Longitude=r.Longitude,RadiusKm=r.RadiusKm};
        await _uow.SavedAnimalSearchFilters.AddAsync(filter); await _uow.SaveChangesAsync(); return filter.Id;
    }
}