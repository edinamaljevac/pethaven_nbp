using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Adoptions.Queries.GetPostAdoptionReports;
public class GetPostAdoptionReportsQuery : IRequest<List<PostAdoptionReportDto>>
{
    public Guid? ApplicationId { get; set; }
    public Guid? ShelterProfileId { get; set; }
    public Guid? AdopterProfileId { get; set; }
    public bool PendingOnly { get; set; }
}
