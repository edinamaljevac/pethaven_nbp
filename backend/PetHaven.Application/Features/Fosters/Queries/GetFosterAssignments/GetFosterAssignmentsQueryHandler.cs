using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Fosters.Queries.GetFosterAssignments;

public class GetFosterAssignmentsQueryHandler : IRequestHandler<GetFosterAssignmentsQuery, List<FosterAssignmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFosterAssignmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FosterAssignmentDto>> Handle(GetFosterAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _unitOfWork.FosterAssignments.GetAllAsync();
        var animals = await _unitOfWork.Animals.GetAllAsync();
        var profiles = await _unitOfWork.FosterProfiles.GetAllAsync();
        var users = await _unitOfWork.Users.GetAllAsync();
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();

        if (request.FosterProfileId.HasValue) assignments = assignments.Where(x => x.FosterProfileId == request.FosterProfileId.Value).ToList();
        if (request.IsActive.HasValue) assignments = assignments.Where(x => x.IsActive == request.IsActive.Value).ToList();
        if (request.ShelterProfileId.HasValue)
        {
            var animalIds = animals.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).Select(x => x.Id).ToHashSet();
            assignments = assignments.Where(x => animalIds.Contains(x.AnimalId)).ToList();
        }

        return assignments.OrderByDescending(x => x.StartDate).Select(assignment =>
        {
            var animal = animals.FirstOrDefault(x => x.Id == assignment.AnimalId);
            var profile = profiles.FirstOrDefault(x => x.Id == assignment.FosterProfileId);
            var user = profile is null ? null : users.FirstOrDefault(x => x.Id == profile.UserId);
            var shelter = animal is null ? null : shelters.FirstOrDefault(x => x.Id == animal.ShelterProfileId);
            var fosterName = string.Join(" ", new[] { user?.FirstName, user?.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

            return new FosterAssignmentDto
            {
                Id = assignment.Id,
                AnimalId = assignment.AnimalId,
                FosterProfileId = assignment.FosterProfileId,
                ShelterProfileId = animal?.ShelterProfileId ?? Guid.Empty,
                AnimalName = animal is null ? assignment.AnimalId.ToString() : $"{animal.Name} - {animal.Species}",
                AnimalSpecies = animal?.Species ?? string.Empty,
                FosterName = string.IsNullOrWhiteSpace(fosterName) ? user?.Email ?? "Foster" : fosterName,
                ShelterName = shelter?.Name ?? string.Empty,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                IsActive = assignment.IsActive,
                Status = assignment.IsActive ? "Active" : "Completed",
                Notes = assignment.Notes
            };
        }).ToList();
    }
}
