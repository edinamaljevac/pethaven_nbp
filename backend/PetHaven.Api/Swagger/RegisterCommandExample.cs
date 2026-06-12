using PetHaven.Application.Features.Auth.Commands.Register;
using PetHaven.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace PetHaven.Api.Swagger;

public class RegisterCommandExample : IExamplesProvider<RegisterCommand>
{
    public RegisterCommand GetExamples() => new()
    {
        FirstName = "Ana",
        LastName = "Petrovic",
        Email = "ana.petrovic@example.com",
        Password = "StrongPass123!",
        Role = UserRole.Adopter
    };
}