using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Profiles.Commands.UpdateMyProfile;
using PetHaven.Application.Features.Profiles.Queries.GetMyProfile;
using PetHaven.Application.Interfaces;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGeocodingService _geocodingService;

    public ProfileController(IMediator mediator, IGeocodingService geocodingService)
    {
        _mediator = mediator;
        _geocodingService = geocodingService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        return Ok(await _mediator.Send(new GetMyProfileQuery { UserId = GetUserId() }));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileCommand command)
    {
        command.UserId = GetUserId();
        return Ok(await _mediator.Send(command));
    }

    [HttpGet("geocode")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> Geocode([FromQuery] string location, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest("Enter a shelter address or location before finding coordinates.");
        }

        var result = await _geocodingService.FindCoordinatesAsync(location, cancellationToken);
        return result is null
            ? NotFound("Coordinates could not be found for the entered location.")
            : Ok(result);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("User id claim is missing or invalid.");
        }

        return userId;
    }
}
