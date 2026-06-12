using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.AnimalCare.Commands.UpsertBehaviorProfile;
using PetHaven.Application.Features.AnimalCare.Commands.UpsertHealthRecord;
using PetHaven.Application.Interfaces;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/animals/{animalId:guid}/care")]
[Authorize(Roles = "Shelter")]
public class AnimalCareController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public AnimalCareController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpPut("health-record")]
    public async Task<IActionResult> UpsertHealthRecord(Guid animalId, UpsertHealthRecordCommand command)
    {
        await EnsureShelterOwnsAnimal(animalId);
        command.AnimalId = animalId;
        return Ok(await _mediator.Send(command));
    }

    [HttpPut("behavior-profile")]
    public async Task<IActionResult> UpsertBehaviorProfile(Guid animalId, UpsertBehaviorProfileCommand command)
    {
        await EnsureShelterOwnsAnimal(animalId);
        command.AnimalId = animalId;
        return Ok(await _mediator.Send(command));
    }

    private async Task EnsureShelterOwnsAnimal(Guid animalId)
    {
        var shelterProfileId = await GetCurrentShelterProfileId();
        var animal = await _unitOfWork.Animals.GetByIdAsync(animalId)
            ?? throw new KeyNotFoundException("Animal not found.");

        if (animal.ShelterProfileId != shelterProfileId)
        {
            throw new UnauthorizedAccessException("You can manage care data only for animals from your shelter.");
        }
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
