using MediatR;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Commands.UpdateAdoptionStatus;

public class UpdateAdoptionStatusCommand : IRequest
{
    public Guid ApplicationId { get; set; }
    public AdoptionApplicationStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
}
