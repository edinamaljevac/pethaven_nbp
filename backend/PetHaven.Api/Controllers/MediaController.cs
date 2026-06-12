using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Media.Commands.AddAnimalPhoto;
using PetHaven.Application.Features.Media.Commands.AddShelterPhoto;
namespace PetHaven.Api.Controllers;
[ApiController]
[Route("api/v1/media")]
[Authorize(Roles = "Shelter,Admin")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;
    public MediaController(IMediator mediator)=>_mediator=mediator;
    [HttpPost("animals/{animalId:guid}/photos")]
    public async Task<IActionResult> AddAnimalPhoto(Guid animalId, AddAnimalPhotoCommand command){command.AnimalId=animalId; return Ok(await _mediator.Send(command));}
    [HttpPost("shelters/{shelterId:guid}/photos")]
    public async Task<IActionResult> AddShelterPhoto(Guid shelterId, AddShelterPhotoCommand command){command.ShelterProfileId=shelterId; return Ok(await _mediator.Send(command));}
}