using MediatR;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.LostFound.Commands.SetLostFoundReportVisibility;

public class SetLostFoundReportVisibilityCommandHandler : IRequestHandler<SetLostFoundReportVisibilityCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetLostFoundReportVisibilityCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(SetLostFoundReportVisibilityCommand request, CancellationToken cancellationToken)
    {
        var report = await _unitOfWork.LostFoundReports.GetByIdAsync(request.ReportId)
            ?? throw new KeyNotFoundException("Lost/found report not found.");

        report.IsHidden = request.IsHidden;
        report.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.LostFoundReports.Update(report);
        await _unitOfWork.SaveChangesAsync();
    }
}
