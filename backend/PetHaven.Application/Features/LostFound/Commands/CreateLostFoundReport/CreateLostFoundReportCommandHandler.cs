using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.LostFound.Commands.CreateLostFoundReport;

public class CreateLostFoundReportCommandHandler : IRequestHandler<CreateLostFoundReportCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateLostFoundReportCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<Guid> Handle(CreateLostFoundReportCommand request, CancellationToken cancellationToken)
    {
        var reportDate = request.ReportDate.Kind == DateTimeKind.Utc
            ? request.ReportDate
            : DateTime.SpecifyKind(request.ReportDate.Date, DateTimeKind.Utc);

        var report = new LostFoundReport { UserId = request.UserId, Type = request.Type, Species = request.Species.Trim(), Breed = request.Breed.Trim(), Color = request.Color.Trim(), Description = request.Description.Trim(), Location = request.Location.Trim(), ReportDate = reportDate, ImageUrl = request.ImageUrl.Trim(), ContactPhone = request.ContactPhone.Trim() };
        await _unitOfWork.LostFoundReports.AddAsync(report); await _unitOfWork.SaveChangesAsync(); return report.Id;
    }
}
