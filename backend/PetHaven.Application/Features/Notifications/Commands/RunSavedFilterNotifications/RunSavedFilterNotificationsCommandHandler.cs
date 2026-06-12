using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.Notifications.Commands.RunSavedFilterNotifications;
public class RunSavedFilterNotificationsCommandHandler : IRequestHandler<RunSavedFilterNotificationsCommand, int>
{
    private readonly IUnitOfWork _uow;
    public RunSavedFilterNotificationsCommandHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task<int> Handle(RunSavedFilterNotificationsCommand r, CancellationToken ct)
    {
        var filters=await _uow.SavedAnimalSearchFilters.GetAllAsync(); var animals=await _uow.Animals.GetAllAsync(); var created=0;
        foreach(var f in filters)
        {
            var matches=animals.Where(a=>a.CreatedAt>=f.LastCheckedAt && a.Status==AnimalStatus.Available).ToList();
            if(!string.IsNullOrWhiteSpace(f.Species)) matches=matches.Where(a=>a.Species.Contains(f.Species,StringComparison.OrdinalIgnoreCase)).ToList();
            if(!string.IsNullOrWhiteSpace(f.Breed)) matches=matches.Where(a=>a.Breed.Contains(f.Breed,StringComparison.OrdinalIgnoreCase)).ToList();
            if(f.Size.HasValue) matches=matches.Where(a=>a.Size==f.Size.Value).ToList();
            if(f.EnergyLevel.HasValue) matches=matches.Where(a=>a.EnergyLevel==f.EnergyLevel.Value).ToList();
            foreach(var a in matches.Take(5)){await _uow.Notifications.AddAsync(new Notification{UserId=f.UserId,Type=NotificationType.NewAnimalMatch,Title="New animal match",Message=$"{a.Name} matches your saved filter '{f.Name}'."}); created++;}
            f.LastCheckedAt=DateTime.UtcNow; _uow.SavedAnimalSearchFilters.Update(f);
        }
        await _uow.SaveChangesAsync(); return created;
    }
}