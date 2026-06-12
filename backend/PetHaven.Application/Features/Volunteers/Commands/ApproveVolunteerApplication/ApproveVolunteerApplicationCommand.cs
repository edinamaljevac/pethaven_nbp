using MediatR;

namespace PetHaven.Application.Features.Volunteers.Commands.ApproveVolunteerApplication;

public class ApproveVolunteerApplicationCommand : IRequest
{
    public Guid ApplicationId { get; set; }
    public bool IsApproved { get; set; } = true;
}