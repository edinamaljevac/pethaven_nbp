using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.AdopterProfiles.Commands.UpsertAdopterProfile;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/adopter-profiles")]
[Authorize]
public class AdopterProfilesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdopterProfilesController(IMediator mediator) => _mediator = mediator;

    [HttpPut]
    public async Task<IActionResult> Upsert(UpsertAdopterProfileCommand command) => Ok(new { Id = await _mediator.Send(command) });
}