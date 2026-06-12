using MediatR;
using Microsoft.Extensions.Options;
using PetHaven.Application.DTOs.Auth;
using PetHaven.Application.Interfaces;
using PetHaven.Application.Settings;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = ResolveFirstName(request),
            LastName = ResolveLastName(request),
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = request.Role
        };

        await _unitOfWork.Users.AddAsync(user);
        await AddRoleProfileAsync(user, request);

        var refreshToken = _jwtTokenService.GenerateRefreshToken(user);
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    private async Task AddRoleProfileAsync(User user, RegisterCommand request)
    {
        switch (request.Role)
        {
            case UserRole.Shelter:
                await _unitOfWork.ShelterProfiles.AddAsync(new ShelterProfile
                {
                    UserId = user.Id,
                    Name = request.ShelterName!.Trim(),
                    Location = request.ShelterLocation?.Trim() ?? string.Empty,
                    ContactPhone = request.ShelterContactPhone?.Trim() ?? string.Empty,
                    Description = request.ShelterDescription?.Trim() ?? string.Empty,
                    Latitude = request.ShelterLatitude,
                    Longitude = request.ShelterLongitude
                });
                break;

            case UserRole.Adopter:
                await _unitOfWork.AdopterProfiles.AddAsync(new AdopterProfile
                {
                    UserId = user.Id,
                    Address = request.AdopterAddress?.Trim() ?? string.Empty,
                    HousingType = request.AdopterHousingType ?? HousingType.Apartment,
                    HouseholdMembers = request.AdopterHouseholdMembers ?? 1,
                    HasChildren = request.AdopterHasChildren ?? false,
                    HasOtherPets = request.AdopterHasOtherPets ?? false,
                    ExperienceWithPets = request.AdopterExperienceWithPets?.Trim() ?? string.Empty,
                    AdoptionReason = request.AdopterAdoptionReason?.Trim() ?? string.Empty
                });
                break;

            case UserRole.Foster:
                await _unitOfWork.FosterProfiles.AddAsync(new FosterProfile
                {
                    UserId = user.Id,
                    PreferredAnimalType = request.FosterPreferredAnimalType?.Trim() ?? string.Empty,
                    Capacity = request.FosterCapacity ?? 1,
                    AvailableFrom = request.FosterAvailableFrom ?? DateTime.UtcNow,
                    AvailableTo = request.FosterAvailableTo ?? DateTime.UtcNow.AddMonths(1)
                });
                break;
        }
    }

    private static string ResolveFirstName(RegisterCommand request)
    {
        return request.Role == UserRole.Shelter
            ? request.ShelterName!.Trim()
            : request.FirstName.Trim();
    }

    private static string ResolveLastName(RegisterCommand request)
    {
        return request.Role == UserRole.Shelter
            ? "Shelter"
            : request.LastName.Trim();
    }
}