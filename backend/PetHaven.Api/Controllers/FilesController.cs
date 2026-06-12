using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Files.Commands.SaveFileAsset;
using PetHaven.Application.Features.Files.Queries.GetFileAssets;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Shelter,Admin")]
    public async Task<IActionResult> GetFiles([FromQuery] GetFileAssetsQuery query) => Ok(await _mediator.Send(query));

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] Guid? uploadedByUserId)
    {
        if (file.Length == 0) return BadRequest("File is empty.");
        await using var stream = file.OpenReadStream();
        return Ok(await _mediator.Send(new SaveFileAssetCommand { Stream = stream, OriginalFileName = file.FileName, ContentType = file.ContentType, SizeBytes = file.Length, UploadedByUserId = uploadedByUserId }));
    }
}
