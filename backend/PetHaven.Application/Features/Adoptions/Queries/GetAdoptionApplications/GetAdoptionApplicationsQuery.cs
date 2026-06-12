using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Queries.GetAdoptionApplications;

public class GetAdoptionApplicationsQuery : IRequest<List<AdoptionApplicationDto>>
{
    public Guid? AnimalId { get; set; }
    public Guid? AdopterProfileId { get; set; }
    public Guid? ShelterProfileId { get; set; }
    public AdoptionApplicationStatus? Status { get; set; }
}
