using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Fosters.Queries.GetFosterProfiles;

public class GetFosterProfilesQueryHandler : IRequestHandler<GetFosterProfilesQuery, List<FosterProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFosterProfilesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FosterProfileDto>> Handle(GetFosterProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _unitOfWork.FosterProfiles.GetAllAsync();
        var assignments = await _unitOfWork.FosterAssignments.GetAllAsync();
        if (request.UserId.HasValue) profiles = profiles.Where(x => x.UserId == request.UserId.Value).ToList();
        if (!string.IsNullOrWhiteSpace(request.PreferredAnimalType)) profiles = profiles.Where(x => x.PreferredAnimalType.Contains(request.PreferredAnimalType, StringComparison.OrdinalIgnoreCase)).ToList();
        if (request.MinimumCapacity.HasValue) profiles = profiles.Where(x => x.Capacity >= request.MinimumCapacity.Value).ToList();

        var users = await _unitOfWork.Users.GetAllAsync();
        return profiles.Select(profile =>
        {
            var user = users.FirstOrDefault(x => x.Id == profile.UserId);
            var name = string.Join(" ", new[] { user?.FirstName, user?.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
            return new FosterProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FosterName = string.IsNullOrWhiteSpace(name) ? user?.Email ?? "Foster" : name,
                FosterEmail = user?.Email ?? string.Empty,
                PreferredAnimalType = profile.PreferredAnimalType,
                Capacity = profile.Capacity,
                ActiveAssignments = assignments.Count(x => x.FosterProfileId == profile.Id && x.IsActive),
                AvailableFrom = profile.AvailableFrom,
                AvailableTo = profile.AvailableTo
            };
        }).ToList();
    }
}
