using MediatR;

namespace PetHaven.Application.Features.Adoptions.Commands.SubmitAdoptionApplication;

public class SubmitAdoptionApplicationCommand : IRequest<Guid>
{
    public Guid AnimalId { get; set; }
    public Guid AdopterProfileId { get; set; }
    public string Notes { get; set; } = string.Empty;
}