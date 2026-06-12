using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Features.Admin.Queries.GetAdminStatistics;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Admin.Queries.GetPlatformStatistics;

public class GetPlatformStatisticsQueryHandler : IRequestHandler<GetPlatformStatisticsQuery, PlatformStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPlatformStatisticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PlatformStatisticsDto> Handle(GetPlatformStatisticsQuery request, CancellationToken cancellationToken)
    {
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();
        var animals = await _unitOfWork.Animals.GetAllAsync();
        var applications = await _unitOfWork.AdoptionApplications.GetAllAsync();
        var assignments = await _unitOfWork.FosterAssignments.GetAllAsync();
        var lostFoundReports = await _unitOfWork.LostFoundReports.GetAllAsync();
        var donations = await _unitOfWork.Donations.GetAllAsync();
        var adoptedApplications = applications.Where(x => x.Status == AdoptionApplicationStatus.Adopted && x.UpdatedAt.HasValue).ToList();

        return new PlatformStatisticsDto
        {
            Totals = await new GetAdminStatisticsQueryHandler(_unitOfWork).Handle(new GetAdminStatisticsQuery(), cancellationToken),
            PendingShelters = shelters.Count(x => !x.IsVerified),
            ActiveFosterPlacements = assignments.Count(x => x.IsActive),
            ResolvedLostFoundReports = lostFoundReports.Count(x => x.IsResolved),
            AverageAdoptionDays = adoptedApplications.Count == 0
                ? 0
                : Math.Round(adoptedApplications.Average(x => (x.UpdatedAt!.Value - x.CreatedAt).TotalDays), 1),
            AnimalsByStatus = Enum.GetValues<AnimalStatus>()
                .Select(status => Point(Format(status.ToString()), animals.Count(x => x.Status == status))).ToList(),
            ApplicationsByStatus = Enum.GetValues<AdoptionApplicationStatus>()
                .Select(status => Point(Format(status.ToString()), applications.Count(x => x.Status == status))).ToList(),
            AdoptionsByMonth = adoptedApplications
                .GroupBy(x => new { x.UpdatedAt!.Value.Year, x.UpdatedAt.Value.Month })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).TakeLast(12)
                .Select(group => Point($"{group.Key.Year}-{group.Key.Month:00}", group.Count())).ToList(),
            DonationsByMonth = donations.Where(x => x.IsPaid)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).TakeLast(12)
                .Select(group => Point($"{group.Key.Year}-{group.Key.Month:00}", group.Sum(x => x.Amount))).ToList()
        };
    }

    private static StatisticsPointDto Point(string label, decimal value) => new() { Label = label, Value = value };

    private static string Format(string value) =>
        System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
}
