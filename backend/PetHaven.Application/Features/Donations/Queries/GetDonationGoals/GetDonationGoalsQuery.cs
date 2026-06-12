using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Donations.Queries.GetDonationGoals;
public class GetDonationGoalsQuery : IRequest<List<DonationGoalDto>>
{
    public Guid? ShelterProfileId { get; set; }
    public bool ActiveOnly { get; set; }
}