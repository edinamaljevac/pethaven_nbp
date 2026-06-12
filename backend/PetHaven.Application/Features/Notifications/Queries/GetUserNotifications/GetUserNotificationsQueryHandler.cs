using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Notifications.Queries.GetUserNotifications;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _unitOfWork;
    public GetUserNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) { _unitOfWork = unitOfWork; _mapper = mapper; }
    public async Task<List<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = (await _unitOfWork.Notifications.GetAllAsync()).ToList();
        if (request.UserId.HasValue) notifications = notifications.Where(x => x.UserId == request.UserId.Value).ToList();
        if (request.UnreadOnly) notifications = notifications.Where(x => !x.IsRead).ToList();

        var users = (await _unitOfWork.Users.GetAllAsync()).ToDictionary(x => x.Id);
        var result = _mapper.Map<List<NotificationDto>>(notifications.OrderByDescending(x => x.CreatedAt));

        foreach (var notification in result)
        {
            if (!users.TryGetValue(notification.UserId, out var user)) continue;

            notification.RecipientName = $"{user.FirstName} {user.LastName}".Trim();
            notification.RecipientEmail = user.Email;
        }

        return result;
    }
}
