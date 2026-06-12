using MediatR;
using PetHaven.Application.Interfaces;
namespace PetHaven.Application.Features.Adoptions.Commands.SubmitPostAdoptionReport;
public class SubmitPostAdoptionReportCommandHandler : IRequestHandler<SubmitPostAdoptionReportCommand>
{
    private readonly IUnitOfWork _uow;
    public SubmitPostAdoptionReportCommandHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task Handle(SubmitPostAdoptionReportCommand r, CancellationToken ct)
    {
        var report = await _uow.PostAdoptionReports.GetByIdAsync(r.ReportId) ?? throw new KeyNotFoundException("Post-adoption report not found.");
        report.ReportText = r.ReportText.Trim(); report.PhotoUrl = r.PhotoUrl.Trim(); report.IsSubmitted = true; report.SubmittedAt = DateTime.UtcNow; report.UpdatedAt = DateTime.UtcNow;
        _uow.PostAdoptionReports.Update(report); await _uow.SaveChangesAsync();
    }
}