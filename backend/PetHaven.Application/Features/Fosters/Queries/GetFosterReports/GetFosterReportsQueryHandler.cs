using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Fosters.Queries.GetFosterReports;

public class GetFosterReportsQueryHandler : IRequestHandler<GetFosterReportsQuery, List<FosterReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFosterReportsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FosterReportDto>> Handle(GetFosterReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _unitOfWork.FosterReports.GetAllAsync();
        var assignments = await _unitOfWork.FosterAssignments.GetAllAsync();
        var animals = await _unitOfWork.Animals.GetAllAsync();
        var profiles = await _unitOfWork.FosterProfiles.GetAllAsync();
        var users = await _unitOfWork.Users.GetAllAsync();
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();

        if (request.FosterAssignmentId.HasValue) reports = reports.Where(x => x.FosterAssignmentId == request.FosterAssignmentId.Value).ToList();
        if (request.FosterProfileId.HasValue)
        {
            var assignmentIds = assignments.Where(x => x.FosterProfileId == request.FosterProfileId.Value).Select(x => x.Id).ToHashSet();
            reports = reports.Where(x => assignmentIds.Contains(x.FosterAssignmentId)).ToList();
        }
        if (request.ShelterProfileId.HasValue)
        {
            var animalIds = animals.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).Select(x => x.Id).ToHashSet();
            var assignmentIds = assignments.Where(x => animalIds.Contains(x.AnimalId)).Select(x => x.Id).ToHashSet();
            reports = reports.Where(x => assignmentIds.Contains(x.FosterAssignmentId)).ToList();
        }

        return reports.OrderByDescending(x => x.ReportDate).Select(report =>
        {
            var assignment = assignments.FirstOrDefault(x => x.Id == report.FosterAssignmentId);
            var animal = assignment is null ? null : animals.FirstOrDefault(x => x.Id == assignment.AnimalId);
            var profile = assignment is null ? null : profiles.FirstOrDefault(x => x.Id == assignment.FosterProfileId);
            var user = profile is null ? null : users.FirstOrDefault(x => x.Id == profile.UserId);
            var shelter = animal is null ? null : shelters.FirstOrDefault(x => x.Id == animal.ShelterProfileId);
            var fosterName = string.Join(" ", new[] { user?.FirstName, user?.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

            return new FosterReportDto
            {
                Id = report.Id,
                FosterAssignmentId = report.FosterAssignmentId,
                AnimalName = animal is null ? string.Empty : $"{animal.Name} - {animal.Species}",
                FosterName = string.IsNullOrWhiteSpace(fosterName) ? user?.Email ?? "Foster" : fosterName,
                ShelterName = shelter?.Name ?? string.Empty,
                BehaviorNotes = report.BehaviorNotes,
                ProgressNotes = report.ProgressNotes,
                PhotoUrl = report.PhotoUrl,
                ReportDate = report.ReportDate
            };
        }).ToList();
    }
}
