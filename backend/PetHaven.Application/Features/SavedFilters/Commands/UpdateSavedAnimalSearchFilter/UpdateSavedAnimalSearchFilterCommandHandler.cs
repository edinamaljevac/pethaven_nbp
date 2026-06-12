using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.SavedFilters.Commands.UpdateSavedAnimalSearchFilter;

public class UpdateSavedAnimalSearchFilterCommandHandler : IRequestHandler<UpdateSavedAnimalSearchFilterCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSavedAnimalSearchFilterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateSavedAnimalSearchFilterCommand request, CancellationToken cancellationToken)
    {
        var filter = await _unitOfWork.SavedAnimalSearchFilters.GetByIdAsync(request.FilterId)
            ?? throw new KeyNotFoundException("Saved filter not found.");

        if (filter.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can edit only your own saved filters.");
        }

        filter.Name = request.Name.Trim();
        filter.Species = Clean(request.Species);
        filter.Breed = Clean(request.Breed);
        filter.City = Clean(request.City);
        filter.Size = request.Size;
        filter.EnergyLevel = request.EnergyLevel;
        filter.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.SavedAnimalSearchFilters.Update(filter);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
