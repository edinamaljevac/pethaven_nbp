using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Application.Features.Adoptions.Commands.GenerateAdoptionContract;
using PetHaven.Application.Features.Adoptions.Commands.RequestPostAdoptionReports;
using PetHaven.Application.Features.Adoptions.Commands.SubmitAdoptionApplication;
using PetHaven.Application.Features.Adoptions.Commands.SubmitPostAdoptionReport;
using PetHaven.Application.Features.Adoptions.Commands.UpdateAdoptionStatus;
using PetHaven.Application.Features.Adoptions.Queries.GetAdoptionContract;
using PetHaven.Application.Features.Adoptions.Queries.GetAdoptionApplications;
using PetHaven.Application.Features.Adoptions.Queries.GetPostAdoptionReports;
using PetHaven.Application.Interfaces;
using System.Security.Claims;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/adoptions")]
[Authorize]
public class AdoptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public AdoptionsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "Adopter,Shelter,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApplications([FromQuery] GetAdoptionApplicationsQuery query)
    {
        if (User.IsInRole("Adopter"))
        {
            query.AdopterProfileId = await GetCurrentAdopterProfileId();
        }
        else if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = "Adopter")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    public async Task<IActionResult> Submit(SubmitAdoptionApplicationCommand command)
    {
        command.AdopterProfileId = await GetCurrentAdopterProfileId();
        return Ok(new { Id = await _mediator.Send(command) });
    }

    [HttpPatch("{applicationId:guid}/status")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> UpdateStatus(Guid applicationId, UpdateAdoptionStatusCommand command)
    {
        await EnsureShelterOwnsApplication(applicationId);
        command.ApplicationId = applicationId;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{applicationId:guid}/contract")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> GenerateContract(Guid applicationId)
    {
        await EnsureShelterOwnsApplication(applicationId);
        return Ok(await _mediator.Send(new GenerateAdoptionContractCommand { ApplicationId = applicationId }));
    }

    [HttpGet("{applicationId:guid}/contract")]
    [Authorize(Roles = "Adopter,Shelter,Admin")]
    public async Task<IActionResult> GetContract(Guid applicationId)
    {
        if (User.IsInRole("Adopter"))
        {
            await EnsureAdopterOwnsApplication(applicationId);
        }
        else if (User.IsInRole("Shelter"))
        {
            await EnsureShelterOwnsApplication(applicationId);
        }

        return Ok(await _mediator.Send(new GetAdoptionContractQuery { ApplicationId = applicationId }));
    }

    [HttpPost("{applicationId:guid}/post-adoption-reports")]
    [Authorize(Roles = "Shelter")]
    public async Task<IActionResult> RequestReports(Guid applicationId)
    {
        await EnsureShelterOwnsApplication(applicationId);
        await _mediator.Send(new RequestPostAdoptionReportsCommand { ApplicationId = applicationId });
        return NoContent();
    }

    [HttpGet("post-adoption-reports")]
    [Authorize(Roles = "Adopter,Shelter,Admin")]
    public async Task<IActionResult> GetReports([FromQuery] GetPostAdoptionReportsQuery query)
    {
        if (User.IsInRole("Adopter"))
        {
            query.AdopterProfileId = await GetCurrentAdopterProfileId();
        }
        else if (User.IsInRole("Shelter"))
        {
            query.ShelterProfileId = await GetCurrentShelterProfileId();
        }

        return Ok(await _mediator.Send(query));
    }

    [HttpPatch("post-adoption-reports/{reportId:guid}/submit")]
    [Authorize(Roles = "Adopter")]
    public async Task<IActionResult> SubmitReport(Guid reportId, SubmitPostAdoptionReportCommand command)
    {
        await EnsureAdopterOwnsReport(reportId);
        command.ReportId = reportId;
        await _mediator.Send(command);
        return NoContent();
    }

    private async Task EnsureShelterOwnsApplication(Guid applicationId)
    {
        var shelterProfileId = await GetCurrentShelterProfileId();
        var application = await _unitOfWork.AdoptionApplications.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException("Adoption application not found.");
        var animal = await _unitOfWork.Animals.GetByIdAsync(application.AnimalId)
            ?? throw new KeyNotFoundException("Animal not found.");

        if (animal.ShelterProfileId != shelterProfileId)
        {
            throw new UnauthorizedAccessException("You can manage only adoption applications for animals from your shelter.");
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

    private async Task<Guid> GetCurrentAdopterProfileId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(value, out var parsed) ? parsed : throw new UnauthorizedAccessException("Invalid user token.");
        var profile = await _unitOfWork.AdopterProfiles.FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Adopter profile not found.");
        return profile.Id;
    }

    private async Task EnsureAdopterOwnsApplication(Guid applicationId)
    {
        var adopterProfileId = await GetCurrentAdopterProfileId();
        var application = await _unitOfWork.AdoptionApplications.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException("Adoption application not found.");

        if (application.AdopterProfileId != adopterProfileId)
        {
            throw new UnauthorizedAccessException("You can view only contracts for your own adoption applications.");
        }
    }

    private async Task EnsureAdopterOwnsReport(Guid reportId)
    {
        var adopterProfileId = await GetCurrentAdopterProfileId();
        var report = await _unitOfWork.PostAdoptionReports.GetByIdAsync(reportId)
            ?? throw new KeyNotFoundException("Post-adoption report not found.");
        var application = await _unitOfWork.AdoptionApplications.GetByIdAsync(report.AdoptionApplicationId)
            ?? throw new KeyNotFoundException("Adoption application not found.");

        if (application.AdopterProfileId != adopterProfileId)
        {
            throw new UnauthorizedAccessException("You can submit only your own post-adoption reports.");
        }
    }
}
