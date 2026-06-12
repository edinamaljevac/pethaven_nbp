using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Features.Fosters.Queries.GetFosterReports;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.Fosters.Commands.SubmitFosterReport;

public class SubmitFosterReportCommandHandler : IRequestHandler<SubmitFosterReportCommand, FosterReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFosterReportCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FosterReportDto> Handle(SubmitFosterReportCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BehaviorNotes)) throw new InvalidOperationException("Behavior Notes are required.");
        if (string.IsNullOrWhiteSpace(request.ProgressNotes)) throw new InvalidOperationException("Progress Notes are required.");

        var assignment = await _unitOfWork.FosterAssignments.GetByIdAsync(request.FosterAssignmentId)
            ?? throw new KeyNotFoundException("Foster assignment not found.");
        if (!assignment.IsActive) throw new InvalidOperationException("Reports can only be submitted for active foster placements.");

        var report = new FosterReport
        {
            FosterAssignmentId = request.FosterAssignmentId,
            BehaviorNotes = request.BehaviorNotes.Trim(),
            ProgressNotes = request.ProgressNotes.Trim(),
            PhotoUrl = request.PhotoUrl.Trim()
        };

        await _unitOfWork.FosterReports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        var reports = await new GetFosterReportsQueryHandler(_unitOfWork).Handle(new GetFosterReportsQuery { FosterAssignmentId = request.FosterAssignmentId }, cancellationToken);
        return reports.First(x => x.Id == report.Id);
    }
}
