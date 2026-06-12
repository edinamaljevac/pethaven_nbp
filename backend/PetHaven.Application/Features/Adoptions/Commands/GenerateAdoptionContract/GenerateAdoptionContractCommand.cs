using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Adoptions.Commands.GenerateAdoptionContract;
public class GenerateAdoptionContractCommand : IRequest<AdoptionContractDto>
{
    public Guid ApplicationId { get; set; }
}