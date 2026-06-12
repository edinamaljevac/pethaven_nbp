using MediatR;
using Microsoft.Extensions.Options;
using PetHaven.Application.DTOs.Auth;
using PetHaven.Application.Interfaces;
using PetHaven.Application.Settings;

namespace PetHaven.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Refresh token user no longer exists.");
        }

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user);
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshToken.Token;

        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }
}