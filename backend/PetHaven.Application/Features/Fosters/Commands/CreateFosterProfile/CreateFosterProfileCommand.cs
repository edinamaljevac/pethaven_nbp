using MediatR;

namespace PetHaven.Application.Features.Fosters.Commands.CreateFosterProfile;

public class CreateFosterProfileCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string PreferredAnimalType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
}