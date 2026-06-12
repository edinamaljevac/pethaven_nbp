using MediatR;
namespace PetHaven.Application.Features.Donations.Commands.CreateDonationGoal;
public class CreateDonationGoalCommand : IRequest<Guid>
{
    public Guid ShelterProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
}