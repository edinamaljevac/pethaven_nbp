using MediatR;
namespace PetHaven.Application.Features.Adoptions.Commands.SubmitPostAdoptionReport;
public class SubmitPostAdoptionReportCommand : IRequest
{
    public Guid ReportId { get; set; }
    public string ReportText { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}