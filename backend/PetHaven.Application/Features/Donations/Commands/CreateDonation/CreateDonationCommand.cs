using MediatR;
namespace PetHaven.Application.Features.Donations.Commands.CreateDonation;
public class CreateDonationCommand : IRequest<Guid>
{
    public Guid ShelterProfileId { get; set; }
    public Guid? DonationGoalId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
}