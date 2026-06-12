using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Fosters.Commands.EndFosterAssignment;

public class EndFosterAssignmentCommand : IRequest<FosterAssignmentDto>
{
    public Guid FosterAssignmentId { get; set; }
}
