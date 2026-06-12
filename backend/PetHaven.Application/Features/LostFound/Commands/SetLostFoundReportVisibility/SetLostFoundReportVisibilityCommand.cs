using MediatR;

namespace PetHaven.Application.Features.LostFound.Commands.SetLostFoundReportVisibility;

public class SetLostFoundReportVisibilityCommand : IRequest
{
    public Guid ReportId { get; set; }
    public bool IsHidden { get; set; }
}
