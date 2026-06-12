using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Notifications.Events;

public class AnimalBecameAvailableNotificationHandler : INotificationHandler<AnimalBecameAvailableNotification>
{
    private readonly IUnitOfWork _unitOfWork;

    public AnimalBecameAvailableNotificationHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(AnimalBecameAvailableNotification request, CancellationToken cancellationToken)
    {
        var animal = await _unitOfWork.Animals.GetByIdAsync(request.AnimalId);
        if (animal is null || animal.Status != AnimalStatus.Available) return;

        var shelter = await _unitOfWork.ShelterProfiles.GetByIdAsync(animal.ShelterProfileId);
        var filters = await _unitOfWork.SavedAnimalSearchFilters.GetAllAsync();
        var existingNotifications = await _unitOfWork.Notifications.GetAllAsync();

        foreach (var filter in filters.Where(filter => Matches(filter, animal, shelter)))
        {
            var message = $"{animal.Name} matches your saved filter '{filter.Name}'.";
            var alreadyCreated = existingNotifications.Any(notification =>
                notification.UserId == filter.UserId
                && notification.Type == NotificationType.NewAnimalMatch
                && notification.Message == message);

            if (alreadyCreated) continue;

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = filter.UserId,
                Type = NotificationType.NewAnimalMatch,
                Title = "New animal match",
                Message = message
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static bool Matches(SavedAnimalSearchFilter filter, Animal animal, ShelterProfile? shelter)
    {
        return MatchesText(animal.Species, filter.Species)
            && MatchesText(animal.Breed, filter.Breed)
            && (!filter.Size.HasValue || animal.Size == filter.Size)
            && (!filter.EnergyLevel.HasValue || animal.EnergyLevel == filter.EnergyLevel)
            && MatchesText(shelter?.Location ?? string.Empty, filter.City);
    }

    private static bool MatchesText(string value, string? filterValue)
    {
        return string.IsNullOrWhiteSpace(filterValue)
            || value.Contains(filterValue, StringComparison.OrdinalIgnoreCase);
    }
}
