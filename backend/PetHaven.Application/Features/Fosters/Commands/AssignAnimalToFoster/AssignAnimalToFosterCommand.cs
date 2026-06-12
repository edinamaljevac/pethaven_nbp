using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Fosters.Commands.AssignAnimalToFoster;
public class AssignAnimalToFosterCommand : IRequest<FosterAssignmentDto>
{
    public Guid AnimalId { get; set; }
    public Guid FosterProfileId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
}