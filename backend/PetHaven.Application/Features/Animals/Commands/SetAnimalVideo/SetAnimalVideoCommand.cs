using MediatR;

namespace PetHaven.Application.Features.Animals.Commands.SetAnimalVideo;

public class SetAnimalVideoCommand : IRequest
{
    public Guid AnimalId { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
}
