using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Adoptions.Queries.GetAdoptionContract;

public class GetAdoptionContractQuery : IRequest<AdoptionContractDto>
{
    public Guid ApplicationId { get; set; }
}
