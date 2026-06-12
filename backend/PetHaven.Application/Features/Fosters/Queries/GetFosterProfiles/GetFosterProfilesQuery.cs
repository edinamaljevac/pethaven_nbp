using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.Fosters.Queries.GetFosterProfiles;

public class GetFosterProfilesQuery : IRequest<List<FosterProfileDto>>
{
    public Guid? UserId { get; set; }
    public string? PreferredAnimalType { get; set; }
    public int? MinimumCapacity { get; set; }
}
