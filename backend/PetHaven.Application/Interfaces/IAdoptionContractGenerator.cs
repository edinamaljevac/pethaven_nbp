using PetHaven.Domain.Entities;

namespace PetHaven.Application.Interfaces;

public interface IAdoptionContractGenerator
{
    Task<string> GenerateAsync(AdoptionApplication application, Animal animal, CancellationToken cancellationToken);
}