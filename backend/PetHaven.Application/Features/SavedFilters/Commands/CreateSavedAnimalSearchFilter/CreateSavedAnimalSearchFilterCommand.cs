using MediatR;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.SavedFilters.Commands.CreateSavedAnimalSearchFilter;
public class CreateSavedAnimalSearchFilterCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public AnimalSize? Size { get; set; }
    public EnergyLevel? EnergyLevel { get; set; }
    public bool SpecialNeedsOnly { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
}