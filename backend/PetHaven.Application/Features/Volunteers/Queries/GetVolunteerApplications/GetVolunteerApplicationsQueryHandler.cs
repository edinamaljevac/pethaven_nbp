using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Volunteers.Queries.GetVolunteerApplications;

public class GetVolunteerApplicationsQueryHandler : IRequestHandler<GetVolunteerApplicationsQuery, List<VolunteerApplicationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVolunteerApplicationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<VolunteerApplicationDto>> Handle(GetVolunteerApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _unitOfWork.VolunteerApplications.GetAllAsync();
        if (request.UserId.HasValue) applications = applications.Where(x => x.UserId == request.UserId.Value).ToList();
        if (request.ShelterProfileId.HasValue) applications = applications.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).ToList();
        if (request.IsApproved.HasValue) applications = applications.Where(x => x.IsApproved == request.IsApproved.Value).ToList();

        var users = await _unitOfWork.Users.GetAllAsync();
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();

        return applications
            .OrderByDescending(x => x.CreatedAt)
            .Select(application =>
            {
                var user = users.FirstOrDefault(x => x.Id == application.UserId);
                var shelter = shelters.FirstOrDefault(x => x.Id == application.ShelterProfileId);
                var status = application.Status;
                if (status == default && application.IsApproved)
                {
                    status = VolunteerApplicationStatus.Approved;
                }

                return new VolunteerApplicationDto
                {
                    Id = application.Id,
                    UserId = application.UserId,
                    ShelterProfileId = application.ShelterProfileId,
                    PreferredActivities = application.PreferredActivities,
                    Availability = application.Availability,
                    Motivation = application.Motivation,
                    IsApproved = application.IsApproved,
                    Status = status.ToString(),
                    ApplicantEmail = user?.Email ?? application.UserId.ToString(),
                    ShelterName = shelter?.Name ?? application.ShelterProfileId.ToString(),
                    CreatedAt = application.CreatedAt
                };
            })
            .ToList();
    }
}
