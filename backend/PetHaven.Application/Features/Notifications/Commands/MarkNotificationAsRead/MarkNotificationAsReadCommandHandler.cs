using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId) ?? throw new KeyNotFoundException("Notification not found.");
        if (!request.IsAdmin && notification.UserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only update your own notifications.");
        }
        notification.IsRead = true; notification.UpdatedAt = DateTime.UtcNow; _unitOfWork.Notifications.Update(notification); await _unitOfWork.SaveChangesAsync();
    }
}
