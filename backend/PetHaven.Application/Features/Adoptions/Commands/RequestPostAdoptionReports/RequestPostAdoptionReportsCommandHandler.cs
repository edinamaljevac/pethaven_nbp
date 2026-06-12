using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Commands.RequestPostAdoptionReports;

public class RequestPostAdoptionReportsCommandHandler : IRequestHandler<RequestPostAdoptionReportsCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestPostAdoptionReportsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(RequestPostAdoptionReportsCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.AdoptionApplications.GetByIdAsync(request.ApplicationId)
            ?? throw new KeyNotFoundException("Adoption application not found.");

        if (application.Status != AdoptionApplicationStatus.Adopted)
        {
            throw new InvalidOperationException("Post-adoption reports can be requested only after the adoption is completed.");
        }

        var existing = (await _unitOfWork.PostAdoptionReports.GetAllAsync())
            .Where(x => x.AdoptionApplicationId == request.ApplicationId)
            .ToList();

        if (existing.Count > 0)
        {
            return;
        }

        var reports = new[]
        {
            new PostAdoptionReport { AdoptionApplicationId = request.ApplicationId, Type = PostAdoptionReportType.After30Days, DueDate = DateTime.UtcNow.AddDays(30) },
            new PostAdoptionReport { AdoptionApplicationId = request.ApplicationId, Type = PostAdoptionReportType.After90Days, DueDate = DateTime.UtcNow.AddDays(90) }
        };

        foreach (var report in reports)
        {
            await _unitOfWork.PostAdoptionReports.AddAsync(report);
        }

        var adopterProfile = await _unitOfWork.AdopterProfiles.GetByIdAsync(application.AdopterProfileId);
        var animal = await _unitOfWork.Animals.GetByIdAsync(application.AnimalId);
        if (adopterProfile is not null)
        {
            foreach (var report in reports)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserId = adopterProfile.UserId,
                    Type = NotificationType.PostAdoptionReportReminder,
                    Title = "Post-adoption report reminder",
                    Message = $"Please submit the {FormatReportType(report.Type)} report for {animal?.Name ?? "your adopted animal"} by {report.DueDate:dd/MM/yyyy}."
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static string FormatReportType(PostAdoptionReportType type)
    {
        return type == PostAdoptionReportType.After30Days ? "30-day" : "90-day";
    }
}
