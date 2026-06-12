using PetHaven.Application.Features.Shelters.Commands.CreateShelter;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Swagger;

public class CreateShelterCommandExample : IExamplesProvider<CreateShelterCommand>
{
    public CreateShelterCommand GetExamples() => new()
    {
        UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Name = "PetHaven Belgrade Shelter",
        Location = "Belgrade",
        ContactPhone = "+38164111222",
        Description = "Verified animal shelter in Belgrade.",
        Latitude = 44.8125,
        Longitude = 20.4612
    };
}