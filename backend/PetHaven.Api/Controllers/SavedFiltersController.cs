using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.SavedFilters.Commands.CreateSavedAnimalSearchFilter;
using PetHaven.Application.Features.SavedFilters.Commands.DeleteSavedAnimalSearchFilter;
using PetHaven.Application.Features.SavedFilters.Commands.UpdateSavedAnimalSearchFilter;
using PetHaven.Application.Features.SavedFilters.Queries.GetSavedAnimalSearchFilters;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/saved-filters")]
[Authorize]
public class SavedFiltersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavedFiltersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetSavedAnimalSearchFiltersQuery query)
    {
        if (!User.IsInRole("Admin"))
        {
            query.UserId = GetCurrentUserId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = "Adopter")]
    public async Task<IActionResult> Create(CreateSavedAnimalSearchFilterCommand command)
    {
        command.UserId = GetCurrentUserId();
        return Ok(new { Id = await _mediator.Send(command) });
    }

    [HttpPut("{filterId:guid}")]
    [Authorize(Roles = "Adopter")]
    public async Task<IActionResult> Update(Guid filterId, UpdateSavedAnimalSearchFilterCommand command)
    {
        command.FilterId = filterId;
        command.UserId = GetCurrentUserId();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{filterId:guid}")]
    [Authorize(Roles = "Adopter")]
    public async Task<IActionResult> Delete(Guid filterId)
    {
        await _mediator.Send(new DeleteSavedAnimalSearchFilterCommand
        {
            FilterId = filterId,
            UserId = GetCurrentUserId()
        });
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : throw new UnauthorizedAccessException("Invalid user token.");
    }
}
