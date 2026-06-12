using MediatR;

namespace PetHaven.Application.Features.Notifications.Events;

public sealed record AnimalBecameAvailableNotification(Guid AnimalId) : INotification;
