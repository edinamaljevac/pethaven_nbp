using MediatR;

namespace PetHaven.Application.Features.Volunteers.Commands.SubmitVolunteerApplication;

public class SubmitVolunteerApplicationCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public Guid ShelterProfileId { get; set; }
    public string PreferredActivities { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
}