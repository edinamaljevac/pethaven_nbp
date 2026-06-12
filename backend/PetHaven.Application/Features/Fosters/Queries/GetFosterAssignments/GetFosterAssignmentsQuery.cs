using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Fosters.Queries.GetFosterAssignments;

public class GetFosterAssignmentsQuery : IRequest<List<FosterAssignmentDto>>
{
    public Guid? FosterProfileId { get; set; }
    public Guid? ShelterProfileId { get; set; }
    public bool? IsActive { get; set; }
}
