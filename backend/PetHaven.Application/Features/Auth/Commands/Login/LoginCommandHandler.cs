using MediatR;
using Microsoft.Extensions.Options;
using PetHaven.Application.DTOs.Auth;
using PetHaven.Application.Interfaces;
using PetHaven.Application.Settings;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(
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

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Role == UserRole.Shelter)
        {
            var shelterProfile = await _unitOfWork.ShelterProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (shelterProfile is null || !shelterProfile.IsVerified)
            {
                throw new UnauthorizedAccessException("Your shelter account is waiting for admin verification. You will be able to log in after your documents are approved.");
            }
        }

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
}
