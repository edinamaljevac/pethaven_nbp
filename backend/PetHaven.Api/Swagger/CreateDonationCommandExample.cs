using PetHaven.Application.Features.Donations.Commands.CreateDonation;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Swagger;

public class CreateDonationCommandExample : IExamplesProvider<CreateDonationCommand>
{
    public CreateDonationCommand GetExamples() => new()
    {
        ShelterProfileId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        DonationGoalId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Amount = 2500,
        Purpose = "Food and medicine"
    };
}