using MediatR;

namespace PetHaven.Application.Features.LostFound.Commands.ResolveLostFoundReport;

public class ResolveLostFoundReportCommand : IRequest
{
    public Guid ReportId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsShelter { get; set; }
}
