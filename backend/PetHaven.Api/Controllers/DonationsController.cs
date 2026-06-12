using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Api.Swagger;
using PetHaven.Application.Features.Donations.Commands.CreateDonation;
using PetHaven.Application.Features.Donations.Commands.CreateDonationGoal;
using PetHaven.Application.Features.Donations.Queries.GetDonationGoals;
using PetHaven.Application.Features.Donations.Queries.GetDonations;
using PetHaven.Application.Interfaces;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/donations")]
public class DonationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public DonationsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "Shelter,Admin")]
    public async Task<IActionResult> GetDonations([FromQuery] GetDonationsQuery query)
    {
        if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost("mock-payment")]
    [AllowAnonymous]
    [SwaggerRequestExample(typeof(CreateDonationCommand), typeof(CreateDonationCommandExample))]
    public async Task<IActionResult> Donate(CreateDonationCommand command)
    {
        var userId = TryGetUserId();
        if (userId.HasValue)
        {
            command.UserId = userId;
        }

        return Ok(new { Id = await _mediator.Send(command), Message = "Mock payment accepted." });
    }

    [HttpGet("goals")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGoals([FromQuery] GetDonationGoalsQuery query) => Ok(await _mediator.Send(query));

    [HttpPost("goals")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> CreateGoal(CreateDonationGoalCommand command)
    {
        command.ShelterProfileId = await GetCurrentShelterProfileId();
        return Ok(new { Id = await _mediator.Send(command) });
    }

    private Guid? TryGetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private async Task<Guid> GetCurrentShelterProfileId()
    {
        var userId = TryGetUserId() ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Shelter profile not found.");
        return profile.Id;
    }
}
