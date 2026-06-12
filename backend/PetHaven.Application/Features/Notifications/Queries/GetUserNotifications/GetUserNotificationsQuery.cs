using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQuery : IRequest<List<NotificationDto>>
{
    public Guid? UserId { get; set; }
    public bool UnreadOnly { get; set; }
}
