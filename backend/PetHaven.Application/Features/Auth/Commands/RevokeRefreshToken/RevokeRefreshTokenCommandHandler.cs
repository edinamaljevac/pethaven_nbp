using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeRefreshTokenCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken is null)
        {
            return Unit.Value;
        }

        if (!storedToken.IsRevoked)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(storedToken);
            await _unitOfWork.SaveChangesAsync();
        }

        return Unit.Value;
    }
}