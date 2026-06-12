using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Adoptions.Queries.GetAdoptionApplications;

public class GetAdoptionApplicationsQueryHandler : IRequestHandler<GetAdoptionApplicationsQuery, List<AdoptionApplicationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdoptionApplicationsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<AdoptionApplicationDto>> Handle(GetAdoptionApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _unitOfWork.AdoptionApplications.GetAllAsync();
        var animals = await _unitOfWork.Animals.GetAllAsync();
        var adopters = await _unitOfWork.AdopterProfiles.GetAllAsync();
        var users = await _unitOfWork.Users.GetAllAsync();
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();

        if (request.AnimalId.HasValue) applications = applications.Where(x => x.AnimalId == request.AnimalId.Value).ToList();
        if (request.AdopterProfileId.HasValue) applications = applications.Where(x => x.AdopterProfileId == request.AdopterProfileId.Value).ToList();
        if (request.Status.HasValue) applications = applications.Where(x => x.Status == request.Status.Value).ToList();
        if (request.ShelterProfileId.HasValue)
        {
            var shelterAnimalIds = animals.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).Select(x => x.Id).ToHashSet();
            applications = applications.Where(x => shelterAnimalIds.Contains(x.AnimalId)).ToList();
        }

        return applications.OrderByDescending(x => x.CreatedAt).Select(application =>
        {
            var animal = animals.FirstOrDefault(x => x.Id == application.AnimalId);
            var adopter = adopters.FirstOrDefault(x => x.Id == application.AdopterProfileId);
            var user = adopter is null ? null : users.FirstOrDefault(x => x.Id == adopter.UserId);
            var shelter = animal is null ? null : shelters.FirstOrDefault(x => x.Id == animal.ShelterProfileId);

            return new AdoptionApplicationDto
            {
                Id = application.Id,
                AnimalId = application.AnimalId,
                AnimalName = animal?.Name ?? string.Empty,
                AnimalSpecies = animal?.Species ?? string.Empty,
                ShelterName = shelter?.Name ?? string.Empty,
                AdopterProfileId = application.AdopterProfileId,
                AdopterName = user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim(),
                AdopterEmail = user?.Email ?? string.Empty,
                Status = application.Status,
                Notes = application.Notes,
                CreatedAt = application.CreatedAt
            };
        }).ToList();
    }
}
