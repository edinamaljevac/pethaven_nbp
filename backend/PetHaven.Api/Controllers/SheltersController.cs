using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Shelters.Commands.CreateShelter;
using PetHaven.Application.Features.Shelters.Commands.VerifyShelter;
using PetHaven.Application.Features.Shelters.Queries.GetShelters;
using PetHaven.Api.Middleware;
using PetHaven.Api.Swagger;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/shelters")]
public class SheltersController : ControllerBase
{
    private readonly IMediator _mediator;
    public SheltersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetShelters([FromQuery] GetSheltersQuery query) => Ok(await _mediator.Send(query));

    [HttpGet("near-me")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSheltersNearMe([FromQuery] double radiusKm = 25)
    {
        var geo = HttpContext.Items[GeoLocationEnrichmentMiddleware.ContextKey] as RequestGeo;
        var query = new GetSheltersQuery { IsVerified = true };

        if (geo?.Latitude is not null && geo.Longitude is not null)
        {
            query.Latitude = geo.Latitude;
            query.Longitude = geo.Longitude;
            query.RadiusKm = radiusKm;
        }
        else if (!string.IsNullOrWhiteSpace(geo?.City) && geo.City != "Unknown")
        {
            query.Location = geo.City;
        }

        var shelters = await _mediator.Send(query);
        return Ok(new
        {
            Geo = geo,
            RadiusKm = radiusKm,
            Shelters = shelters
        });
    }

    [HttpPost]
    [Authorize(Roles = "Shelter,Admin")]
    [SwaggerRequestExample(typeof(CreateShelterCommand), typeof(CreateShelterCommandExample))]
    public async Task<IActionResult> CreateShelter(CreateShelterCommand command) => Ok(new { Id = await _mediator.Send(command) });

    [HttpPatch("{shelterId:guid}/verification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> VerifyShelter(Guid shelterId, VerifyShelterCommand command)
    {
        command.ShelterId = shelterId;
        await _mediator.Send(command);
        return NoContent();
    }
}
