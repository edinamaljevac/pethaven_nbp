using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Fosters.Commands.AssignAnimalToFoster;
using PetHaven.Application.Features.Fosters.Commands.CreateFosterProfile;
using PetHaven.Application.Features.Fosters.Commands.EndFosterAssignment;
using PetHaven.Application.Features.Fosters.Commands.SubmitFosterReport;
using PetHaven.Application.Features.Fosters.Queries.GetFosterAssignments;
using PetHaven.Application.Features.Fosters.Queries.GetFosterProfiles;
using PetHaven.Application.Features.Fosters.Queries.GetFosterReports;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/fosters")]
[Authorize]
public class FostersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public FostersController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "Shelter,Admin")]
    public async Task<IActionResult> GetProfiles([FromQuery] GetFosterProfilesQuery query) => Ok(await _mediator.Send(query));

    [HttpGet("me")]
    [Authorize(Roles = "Foster")]
    public async Task<IActionResult> GetMyProfile()
    {
        var profiles = await _mediator.Send(new GetFosterProfilesQuery { UserId = GetUserId() });
        return Ok(profiles.FirstOrDefault());
    }

    [HttpPost]
    [Authorize(Roles = "Foster")]
    public async Task<IActionResult> CreateProfile(CreateFosterProfileCommand command)
    {
        command.UserId = GetUserId();
        return Ok(new { Id = await _mediator.Send(command) });
    }

    [HttpGet("assignments")]
    [Authorize(Roles = "Foster,Shelter,Admin")]
    public async Task<IActionResult> GetAssignments([FromQuery] GetFosterAssignmentsQuery query)
    {
        if (User.IsInRole("Foster"))
        {
            var profile = await GetCurrentFosterProfile();
            query.FosterProfileId = profile.Id;
        }

        if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost("assignments")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> Assign(AssignAnimalToFosterCommand command)
    {
        var shelterProfileId = await GetCurrentShelterProfileId();
        var animal = await _unitOfWork.Animals.GetByIdAsync(command.AnimalId)
            ?? throw new KeyNotFoundException("Animal not found.");
        if (animal.ShelterProfileId != shelterProfileId) return Forbid();
        if (animal.Status != AnimalStatus.Available) throw new InvalidOperationException("Only available animals can be assigned to foster care.");

        return Ok(await _mediator.Send(command));
    }

    [HttpPatch("assignments/{assignmentId:guid}/end")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> EndAssignment(Guid assignmentId)
    {
        var shelterProfileId = await GetCurrentShelterProfileId();
        var assignment = await _unitOfWork.FosterAssignments.GetByIdAsync(assignmentId)
            ?? throw new KeyNotFoundException("Foster assignment not found.");
        var animal = await _unitOfWork.Animals.GetByIdAsync(assignment.AnimalId)
            ?? throw new KeyNotFoundException("Animal not found.");
        if (animal.ShelterProfileId != shelterProfileId) return Forbid();

        return Ok(await _mediator.Send(new EndFosterAssignmentCommand { FosterAssignmentId = assignmentId }));
    }

    [HttpGet("reports")]
    [Authorize(Roles = "Foster,Shelter,Admin")]
    public async Task<IActionResult> Reports([FromQuery] GetFosterReportsQuery query)
    {
        if (User.IsInRole("Foster"))
        {
            var profile = await GetCurrentFosterProfile();
            query.FosterProfileId = profile.Id;
        }

        if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost("reports")]
    [Authorize(Roles = "Foster")]
    public async Task<IActionResult> SubmitReport(SubmitFosterReportCommand command)
    {
        var profile = await GetCurrentFosterProfile();
        var assignment = await _unitOfWork.FosterAssignments.GetByIdAsync(command.FosterAssignmentId)
            ?? throw new KeyNotFoundException("Foster assignment not found.");
        if (assignment.FosterProfileId != profile.Id) return Forbid();
        if (!assignment.IsActive) throw new InvalidOperationException("Reports can only be submitted for active foster placements.");

        return Ok(await _mediator.Send(command));
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("User is not authenticated.");
    }

    private async Task<PetHaven.Domain.Entities.FosterProfile> GetCurrentFosterProfile()
    {
        var userId = GetUserId();
        return await _unitOfWork.FosterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Foster profile not found.");
    }

    private async Task<Guid> GetCurrentShelterProfileId()
    {
        var userId = GetUserId();
        var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Shelter profile not found.");
        return profile.Id;
    }
}
