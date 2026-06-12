using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using PetHaven.Application.Features.Notifications.Queries.GetUserNotifications;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "Adopter")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUserNotificationsQuery query)
    {
        query.UserId = GetCurrentUserId();

        return Ok(await _mediator.Send(query));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand
        {
            NotificationId = notificationId,
            CurrentUserId = GetCurrentUserId(),
            IsAdmin = false
        });
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("Invalid user token.");
    }
}
