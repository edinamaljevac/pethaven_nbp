using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Api.Swagger;
using PetHaven.Application.Features.Animals.Commands.CreateAnimal;
using PetHaven.Application.Features.Animals.Commands.SetAnimalVideo;
using PetHaven.Application.Features.Animals.Queries.GetAllAnimals;
using PetHaven.Application.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/animals")]
public class AnimalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public AnimalsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllAnimals([FromQuery] GetAllAnimalsQuery query)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }
        else if (!(User.Identity?.IsAuthenticated == true && User.IsInRole("Admin")))
        {
            query.PublicAvailableOnly = true;
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = "Shelter")]
    [SwaggerRequestExample(typeof(CreateAnimalCommand), typeof(CreateAnimalCommandExample))]
    public async Task<IActionResult> CreateAnimal(CreateAnimalCommand command)
    {
        command.ShelterProfileId = await GetCurrentShelterProfileId();
        var animalId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllAnimals), new { id = animalId }, new { Id = animalId, Message = "Animal created successfully." });
    }

    [HttpPatch("{animalId:guid}/video")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> SetVideo(Guid animalId, SetAnimalVideoCommand command)
    {
        var animal = await _unitOfWork.Animals.GetByIdAsync(animalId)
            ?? throw new KeyNotFoundException("Animal not found.");
        if (animal.ShelterProfileId != await GetCurrentShelterProfileId()) return Forbid();

        command.AnimalId = animalId;
        await _mediator.Send(command);
        return NoContent();
    }

    private async Task<Guid> GetCurrentShelterProfileId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(value, out var parsed) ? parsed : throw new UnauthorizedAccessException("Invalid user token.");
        var profile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Shelter profile not found.");
        return profile.Id;
    }
}
