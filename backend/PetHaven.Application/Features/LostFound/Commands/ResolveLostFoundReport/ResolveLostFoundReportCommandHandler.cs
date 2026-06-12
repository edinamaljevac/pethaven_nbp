using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.LostFound.Commands.ResolveLostFoundReport;

public class ResolveLostFoundReportCommandHandler : IRequestHandler<ResolveLostFoundReportCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public ResolveLostFoundReportCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task Handle(ResolveLostFoundReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _unitOfWork.LostFoundReports.GetByIdAsync(request.ReportId) ?? throw new KeyNotFoundException("Lost/found report not found.");
        if (!request.IsShelter && report.UserId != request.CurrentUserId) throw new UnauthorizedAccessException("You can resolve only your own reports.");
        if (report.IsResolved) throw new InvalidOperationException("This report is already resolved.");
        report.IsResolved = true; report.UpdatedAt = DateTime.UtcNow; _unitOfWork.LostFoundReports.Update(report); await _unitOfWork.SaveChangesAsync();
    }
}
