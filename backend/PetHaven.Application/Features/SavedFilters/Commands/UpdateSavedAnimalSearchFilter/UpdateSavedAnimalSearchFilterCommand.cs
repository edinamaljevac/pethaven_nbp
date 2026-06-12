using MediatR;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.SavedFilters.Commands.UpdateSavedAnimalSearchFilter;

public class UpdateSavedAnimalSearchFilterCommand : IRequest
{
    public Guid FilterId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public AnimalSize? Size { get; set; }
    public EnergyLevel? EnergyLevel { get; set; }
    public string? City { get; set; }
}
