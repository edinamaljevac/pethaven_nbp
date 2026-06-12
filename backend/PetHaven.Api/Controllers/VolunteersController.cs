using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Volunteers.Commands.ApproveVolunteerApplication;
using PetHaven.Application.Features.Volunteers.Commands.SubmitVolunteerApplication;
using PetHaven.Application.Features.Volunteers.Queries.GetVolunteerApplications;
using PetHaven.Application.Interfaces;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/volunteers")]
[Authorize]
public class VolunteersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public VolunteersController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("applications")]
    [Authorize(Roles = "Adopter,Shelter,Admin")]
    public async Task<IActionResult> GetApplications([FromQuery] GetVolunteerApplicationsQuery query)
    {
        if (User.IsInRole("Adopter"))
        {
            query.UserId = GetUserId();
        }

        if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost("applications")]
    [Authorize(Roles = "Adopter")]
    public async Task<IActionResult> Submit(SubmitVolunteerApplicationCommand command)
    {
        command.UserId = GetUserId();
        return Ok(new { Id = await _mediator.Send(command) });
    }

    [HttpPatch("applications/{applicationId:guid}/approval")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> Approve(Guid applicationId, ApproveVolunteerApplicationCommand command)
    {
        var shelterProfileId = await GetCurrentShelterProfileId();
        var application = await _unitOfWork.VolunteerApplications.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException("Volunteer application not found.");
        if (application.ShelterProfileId != shelterProfileId)
        {
            return Forbid();
        }

        command.ApplicationId = applicationId;
        await _mediator.Send(command);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    private async Task<Guid> GetCurrentShelterProfileId()
    {
        var userId = GetUserId();
        var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Shelter profile not found.");
        return profile.Id;
    }
}
