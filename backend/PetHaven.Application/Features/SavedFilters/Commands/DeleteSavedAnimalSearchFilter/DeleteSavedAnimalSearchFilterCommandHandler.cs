using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.SavedFilters.Commands.DeleteSavedAnimalSearchFilter;

public class DeleteSavedAnimalSearchFilterCommandHandler : IRequestHandler<DeleteSavedAnimalSearchFilterCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSavedAnimalSearchFilterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteSavedAnimalSearchFilterCommand request, CancellationToken cancellationToken)
    {
        var filter = await _unitOfWork.SavedAnimalSearchFilters.GetByIdAsync(request.FilterId)
            ?? throw new KeyNotFoundException("Saved filter not found.");

        if (filter.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can delete only your own saved filters.");
        }

        _unitOfWork.SavedAnimalSearchFilters.Delete(filter);
        await _unitOfWork.SaveChangesAsync();
    }
}
