using MediatR;

namespace PetHaven.Application.Features.Shelters.Commands.VerifyShelter;

public class VerifyShelterCommand : IRequest
{
    public Guid ShelterId { get; set; }
    public bool IsVerified { get; set; } = true;
}