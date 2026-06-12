using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetHaven.Api.Middleware;
using PetHaven.Api.Swagger;
using PetHaven.Application.Features.Auth.Commands.Login;
using PetHaven.Application.Features.Auth.Commands.RefreshToken;
using PetHaven.Application.Features.Auth.Commands.Register;
using PetHaven.Application.Features.Auth.Commands.RevokeRefreshToken;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IFileStorageService _fileStorageService;

    public AuthController(IMediator mediator, IUnitOfWork unitOfWork, IPasswordService passwordService, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("register")]
    [SwaggerRequestExample(typeof(RegisterCommand), typeof(RegisterCommandExample))]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        if (command.Role == UserRole.Shelter)
        {
            return BadRequest("Shelter registration requires verification documents. Use the shelter registration form.");
        }

        return Ok(await _mediator.Send(command));
    }

    [HttpPost("register-shelter")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterShelter([FromForm] ShelterRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ShelterName))
        {
            return BadRequest("Email, password and shelter name are required.");
        }

        if (request.Documents is null || request.Documents.Count == 0)
        {
            return BadRequest("At least one verification document is required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (existingUser is not null)
        {
            return BadRequest("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.ShelterName.Trim(),
            LastName = "Shelter",
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.Shelter
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.ShelterProfiles.AddAsync(new ShelterProfile
        {
            UserId = user.Id,
            Name = request.ShelterName.Trim(),
            IsVerified = false
        });

        foreach (var document in request.Documents.Where(x => x.Length > 0))
        {
            await using var stream = document.OpenReadStream();
            var asset = await _fileStorageService.SaveAsync(stream, document.FileName, document.ContentType, document.Length, user.Id, cancellationToken);
            await _unitOfWork.FileAssets.AddAsync(asset);
        }

        var admins = (await _unitOfWork.Users.GetAllAsync()).Where(x => x.Role == UserRole.Admin).ToList();
        foreach (var admin in admins)
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = admin.Id,
                Type = NotificationType.ShelterVerification,
                Title = "New shelter verification request",
                Message = $"{request.ShelterName.Trim()} submitted verification documents and is waiting for admin review."
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { Message = "Shelter registration submitted. You will be able to log in after admin verification." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var response = await _mediator.Send(command);
        await TrackLoginLocation(command.Email);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command) => Ok(await _mediator.Send(command));

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken(RevokeRefreshTokenCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    private async Task TrackLoginLocation(string email)
    {
        var geo = HttpContext.Items[GeoLocationEnrichmentMiddleware.ContextKey] as RequestGeo;
        if (geo is null)
        {
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user is null)
        {
            return;
        }

        var previousLogin = (await _unitOfWork.LoginEvents.GetAllAsync())
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (IsSuspiciousLocationChange(previousLogin, geo))
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = user.Id,
                Type = NotificationType.SecurityAlert,
                Title = "New login location detected",
                Message = $"Your account usually logged in from {FormatLocation(previousLogin!)}, but this login came from {FormatLocation(geo)}."
            });
        }

        await _unitOfWork.LoginEvents.AddAsync(new LoginEvent
        {
            UserId = user.Id,
            Country = geo.Country,
            Region = geo.Region,
            City = geo.City,
            IpAddress = geo.IpAddress
        });

        await _unitOfWork.SaveChangesAsync();
    }

    private static bool IsSuspiciousLocationChange(LoginEvent? previousLogin, RequestGeo geo)
    {
        if (previousLogin is null || geo.Country == "Unknown")
        {
            return false;
        }

        if (!string.Equals(previousLogin.Country, geo.Country, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return previousLogin.Country != "Unknown"
            && previousLogin.City != "Unknown"
            && geo.City != "Unknown"
            && !string.Equals(previousLogin.City, geo.City, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatLocation(LoginEvent login)
    {
        return string.Join(", ", new[] { login.City, login.Region, login.Country }.Where(IsKnown));
    }

    private static string FormatLocation(RequestGeo geo)
    {
        return string.Join(", ", new[] { geo.City, geo.Region, geo.Country }.Where(IsKnown));
    }

    private static bool IsKnown(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value != "Unknown";
    }
}

public class ShelterRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ShelterName { get; set; } = string.Empty;
    public List<IFormFile> Documents { get; set; } = [];
}
