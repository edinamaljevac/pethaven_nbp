using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Fosters.Queries.GetFosterReports;
public class GetFosterReportsQuery : IRequest<List<FosterReportDto>>
{
    public Guid? FosterAssignmentId { get; set; }
    public Guid? FosterProfileId { get; set; }
    public Guid? ShelterProfileId { get; set; }
}
