using MediatR;

namespace PetHaven.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommand : IRequest
{
    public Guid NotificationId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }
}
