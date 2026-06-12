using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Admin.Commands.ModerateAnimal;
using PetHaven.Application.Features.Admin.Commands.ModerateContent;
using PetHaven.Application.Features.Admin.Queries.GetAdminStatistics;
using PetHaven.Application.Features.Admin.Queries.GetPlatformStatistics;
namespace PetHaven.Api.Controllers;
[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminController(IMediator mediator)=>_mediator=mediator;
    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics()=>Ok(await _mediator.Send(new GetAdminStatisticsQuery()));
    [HttpGet("statistics/dashboard")]
    public async Task<IActionResult> DashboardStatistics()=>Ok(await _mediator.Send(new GetPlatformStatisticsQuery()));
    [HttpPatch("animals/{animalId:guid}/moderation")]
    public async Task<IActionResult> ModerateAnimal(Guid animalId, ModerateAnimalCommand command){command.AnimalId=animalId; await _mediator.Send(command); return NoContent();}
    [HttpPost("moderation")]
    public async Task<IActionResult> ModerateContent(ModerateContentCommand command){await _mediator.Send(command); return NoContent();}
}
