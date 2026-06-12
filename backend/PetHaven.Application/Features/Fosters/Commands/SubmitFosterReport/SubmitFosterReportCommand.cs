using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Fosters.Commands.SubmitFosterReport;
public class SubmitFosterReportCommand : IRequest<FosterReportDto>
{
    public Guid FosterAssignmentId { get; set; }
    public string BehaviorNotes { get; set; } = string.Empty;
    public string ProgressNotes { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}