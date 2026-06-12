using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Donations.Queries.GetDonations;

public class GetDonationsQuery : IRequest<List<DonationDto>>
{
    public Guid? ShelterProfileId { get; set; }
}