using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.LostFound.Queries.FindLostFoundMatches;
public class FindLostFoundMatchesQuery : IRequest<List<LostFoundMatchDto>>
{
    public Guid ReportId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsShelter { get; set; }
}
