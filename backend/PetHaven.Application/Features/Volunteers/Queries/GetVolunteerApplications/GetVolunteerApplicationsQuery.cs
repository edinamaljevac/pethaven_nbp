using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Volunteers.Queries.GetVolunteerApplications;

public class GetVolunteerApplicationsQuery : IRequest<List<VolunteerApplicationDto>>
{
    public Guid? UserId { get; set; }
    public Guid? ShelterProfileId { get; set; }
    public bool? IsApproved { get; set; }
}
