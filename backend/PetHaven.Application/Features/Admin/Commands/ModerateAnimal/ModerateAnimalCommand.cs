using MediatR;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.Admin.Commands.ModerateAnimal;
public class ModerateAnimalCommand : IRequest
{
    public Guid AnimalId { get; set; }
    public AnimalStatus Status { get; set; }
}