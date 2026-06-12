using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Animals.Queries.GetAllAnimals;

public class GetAllAnimalsQuery : IRequest<List<AnimalDto>>
{
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public AnimalSize? Size { get; set; }
    public EnergyLevel? EnergyLevel { get; set; }
    public AnimalStatus? Status { get; set; }
    public bool SpecialNeedsOnly { get; set; }
    public Guid? ShelterProfileId { get; set; }
    public bool PublicAvailableOnly { get; set; }
    public string? SortBy { get; set; }
}
