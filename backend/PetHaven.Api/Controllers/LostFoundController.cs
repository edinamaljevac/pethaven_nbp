using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.LostFound.Commands.CreateLostFoundReport;
using PetHaven.Application.Features.LostFound.Commands.ResolveLostFoundReport;
using PetHaven.Application.Features.LostFound.Commands.SetLostFoundReportVisibility;
using PetHaven.Application.Features.LostFound.Queries.FindLostFoundMatches;
using PetHaven.Application.Features.LostFound.Queries.SearchLostFoundReports;
using PetHaven.Domain.Enums;
using System.Security.Claims;
namespace PetHaven.Api.Controllers;
[ApiController]
[Route("api/v1/lost-found")]
public class LostFoundController : ControllerBase
{
    private readonly IMediator _mediator;
    public LostFoundController(IMediator mediator) => _mediator = mediator;
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] SearchLostFoundReportsQuery query)
    {
        if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Adopter") || User.IsInRole("Foster")))
        {
            query.CurrentUserId = GetCurrentUserId();
            query.OwnReportsOnly = true;
        }
        query.IsAdmin = User.IsInRole("Admin");
        return Ok(await _mediator.Send(query));
    }
    [HttpGet("{reportId:guid}/matches")]
    [Authorize(Roles = "Adopter,Foster,Shelter")]
    public async Task<IActionResult> Matches(Guid reportId)=>Ok(await _mediator.Send(new FindLostFoundMatchesQuery { ReportId = reportId, CurrentUserId = GetCurrentUserId(), IsShelter = User.IsInRole("Shelter") }));
    [HttpPost]
    [Authorize(Roles = "Adopter,Foster,Shelter")]
    public async Task<IActionResult> Create(CreateLostFoundReportCommand command)
    {
        if (User.IsInRole("Shelter") && command.Type != LostFoundReportType.Found)
        {
            return BadRequest("Shelters can create only found reports.");
        }
        command.UserId = GetCurrentUserId();
        return Ok(new { Id = await _mediator.Send(command) });
    }
    [HttpPatch("{reportId:guid}/resolve")]
    [Authorize(Roles = "Adopter,Foster,Shelter")]
    public async Task<IActionResult> Resolve(Guid reportId){await _mediator.Send(new ResolveLostFoundReportCommand { ReportId = reportId, CurrentUserId = GetCurrentUserId(), IsShelter = User.IsInRole("Shelter") }); return NoContent();}

    [HttpPatch("{reportId:guid}/visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetVisibility(Guid reportId, SetLostFoundReportVisibilityCommand command)
    {
        command.ReportId = reportId;
        await _mediator.Send(command);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("Invalid user token.");
    }
}
