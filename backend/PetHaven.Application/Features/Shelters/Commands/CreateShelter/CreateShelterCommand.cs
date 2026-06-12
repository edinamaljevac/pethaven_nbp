using MediatR;

namespace PetHaven.Application.Features.Shelters.Commands.CreateShelter;

public class CreateShelterCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}