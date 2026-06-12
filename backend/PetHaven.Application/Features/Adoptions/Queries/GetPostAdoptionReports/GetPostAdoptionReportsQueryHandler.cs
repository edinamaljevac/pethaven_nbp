using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Adoptions.Queries.GetPostAdoptionReports;

public class GetPostAdoptionReportsQueryHandler : IRequestHandler<GetPostAdoptionReportsQuery, List<PostAdoptionReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPostAdoptionReportsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<PostAdoptionReportDto>> Handle(GetPostAdoptionReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _unitOfWork.PostAdoptionReports.GetAllAsync();
        var applications = await _unitOfWork.AdoptionApplications.GetAllAsync();
        var animals = await _unitOfWork.Animals.GetAllAsync();
        var adopters = await _unitOfWork.AdopterProfiles.GetAllAsync();
        var users = await _unitOfWork.Users.GetAllAsync();

        if (request.ApplicationId.HasValue) reports = reports.Where(x => x.AdoptionApplicationId == request.ApplicationId.Value).ToList();
        if (request.PendingOnly) reports = reports.Where(x => !x.IsSubmitted).ToList();
        if (request.ShelterProfileId.HasValue)
        {
            var shelterAnimalIds = animals.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).Select(x => x.Id).ToHashSet();
            var shelterApplicationIds = applications.Where(x => shelterAnimalIds.Contains(x.AnimalId)).Select(x => x.Id).ToHashSet();
            reports = reports.Where(x => shelterApplicationIds.Contains(x.AdoptionApplicationId)).ToList();
        }
        if (request.AdopterProfileId.HasValue)
        {
            var adopterApplicationIds = applications.Where(x => x.AdopterProfileId == request.AdopterProfileId.Value).Select(x => x.Id).ToHashSet();
            reports = reports.Where(x => adopterApplicationIds.Contains(x.AdoptionApplicationId)).ToList();
        }

        return reports.OrderBy(x => x.DueDate).Select(report =>
        {
            var application = applications.FirstOrDefault(x => x.Id == report.AdoptionApplicationId);
            var animal = application is null ? null : animals.FirstOrDefault(x => x.Id == application.AnimalId);
            var adopter = application is null ? null : adopters.FirstOrDefault(x => x.Id == application.AdopterProfileId);
            var user = adopter is null ? null : users.FirstOrDefault(x => x.Id == adopter.UserId);

            return new PostAdoptionReportDto
            {
                Id = report.Id,
                AdoptionApplicationId = report.AdoptionApplicationId,
                AnimalName = animal?.Name ?? string.Empty,
                AdopterName = user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim(),
                Type = report.Type,
                DueDate = report.DueDate,
                SubmittedAt = report.SubmittedAt,
                ReportText = report.ReportText,
                PhotoUrl = report.PhotoUrl,
                IsSubmitted = report.IsSubmitted
            };
        }).ToList();
    }
}
