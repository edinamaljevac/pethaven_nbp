using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Animals.Queries.GetAllAnimals;

public class GetAllAnimalsQueryHandler : IRequestHandler<GetAllAnimalsQuery, List<AnimalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllAnimalsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<AnimalDto>> Handle(GetAllAnimalsQuery request, CancellationToken cancellationToken)
    {
        var animals = await _unitOfWork.Animals.GetAllAsync();

        if (request.ShelterProfileId.HasValue) animals = animals.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).ToList();
        if (request.PublicAvailableOnly) animals = animals.Where(x => x.Status == AnimalStatus.Available).ToList();
        if (!string.IsNullOrWhiteSpace(request.Species)) animals = animals.Where(x => x.Species.Contains(request.Species, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(request.Breed)) animals = animals.Where(x => x.Breed.Contains(request.Breed, StringComparison.OrdinalIgnoreCase)).ToList();
        if (request.MinAge.HasValue) animals = animals.Where(x => x.Age >= request.MinAge.Value).ToList();
        if (request.MaxAge.HasValue) animals = animals.Where(x => x.Age <= request.MaxAge.Value).ToList();
        if (request.Size.HasValue) animals = animals.Where(x => x.Size == request.Size.Value).ToList();
        if (request.EnergyLevel.HasValue) animals = animals.Where(x => x.EnergyLevel == request.EnergyLevel.Value).ToList();
        if (request.Status.HasValue) animals = animals.Where(x => x.Status == request.Status.Value).ToList();

        if (request.SpecialNeedsOnly)
        {
            var specialNeedsAnimalIds = (await _unitOfWork.BehaviorProfiles.GetAllAsync())
                .Where(x => x.HasSpecialNeeds)
                .Select(x => x.AnimalId)
                .ToHashSet();

            animals = animals.Where(x => specialNeedsAnimalIds.Contains(x.Id)).ToList();
        }

        var orderedAnimals = request.SortBy switch
        {
            "ageYoungest" => animals.OrderBy(x => x.Age).ThenBy(x => x.IntakeDate).ToList(),
            "ageOldest" => animals.OrderByDescending(x => x.Age).ThenBy(x => x.IntakeDate).ToList(),
            "intakeDateOldest" => animals.OrderBy(x => x.IntakeDate).ThenBy(x => x.CreatedAt).ToList(),
            _ => animals.OrderBy(x => x.IntakeDate).ThenBy(x => x.CreatedAt).ToList(),
        };
        var animalIds = orderedAnimals.Select(x => x.Id).ToHashSet();
        var healthRecords = (await _unitOfWork.HealthRecords.GetAllAsync())
            .Where(x => animalIds.Contains(x.AnimalId))
            .ToDictionary(x => x.AnimalId);
        var behaviorProfiles = (await _unitOfWork.BehaviorProfiles.GetAllAsync())
            .Where(x => animalIds.Contains(x.AnimalId))
            .ToDictionary(x => x.AnimalId);
        var photos = (await _unitOfWork.AnimalPhotos.GetAllAsync())
            .Where(x => animalIds.Contains(x.AnimalId))
            .GroupBy(x => x.AnimalId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(photo => photo.IsMain).ThenBy(photo => photo.CreatedAt).ToList());
        var shelterIds = orderedAnimals.Select(x => x.ShelterProfileId).ToHashSet();
        var shelters = (await _unitOfWork.ShelterProfiles.GetAllAsync())
            .Where(x => shelterIds.Contains(x.Id))
            .ToDictionary(x => x.Id);

        var result = _mapper.Map<List<AnimalDto>>(orderedAnimals);
        foreach (var animal in result)
        {
            animal.DaysInShelter = Math.Max(0, (DateTime.UtcNow.Date - animal.IntakeDate.Date).Days);

            if (shelters.TryGetValue(animal.ShelterProfileId, out var shelter))
            {
                animal.ShelterName = shelter.Name;
            }

            if (healthRecords.TryGetValue(animal.Id, out var healthRecord))
            {
                animal.HealthRecord = _mapper.Map<HealthRecordDto>(healthRecord);
            }

            if (behaviorProfiles.TryGetValue(animal.Id, out var behaviorProfile))
            {
                animal.BehaviorProfile = _mapper.Map<BehaviorProfileDto>(behaviorProfile);
            }

            if (photos.TryGetValue(animal.Id, out var animalPhotos))
            {
                animal.Photos = _mapper.Map<List<MediaFileDto>>(animalPhotos);
            }

            animal.SpecialNeedsReasons = GetSpotlightReasons(animal);
            animal.IsSpecialNeedsSpotlight = animal.SpecialNeedsReasons.Count > 0;
        }

        return result;
    }

    private static List<string> GetSpotlightReasons(AnimalDto animal)
    {
        var reasons = new List<string>();
        var health = animal.HealthRecord;
        var behavior = animal.BehaviorProfile;

        if (behavior?.HasSpecialNeeds == true) reasons.Add("special needs");
        if (HasMeaningfulHealthValue(health?.ChronicDiseases)) reasons.Add("chronic illness");
        if (HasMeaningfulHealthValue(health?.Medications)) reasons.Add("medication");
        if (animal.Age >= 7) reasons.Add("senior");
        if (animal.DaysInShelter >= 90) reasons.Add($"{animal.DaysInShelter} days");
        if (animal.EnergyLevel == EnergyLevel.Low && behavior is not null && (!behavior.GoodWithChildren || !behavior.GoodWithDogs || !behavior.GoodWithCats))
        {
            reasons.Add("needs calm home");
        }

        return reasons.Distinct().ToList();
    }

    private static bool HasMeaningfulHealthValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is not "none" and not "no" and not "n/a" and not "na" and not "-" and not "nothing";
    }
}
