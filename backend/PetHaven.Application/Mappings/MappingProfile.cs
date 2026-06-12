using AutoMapper;
using PetHaven.Application.DTOs;
using PetHaven.Application.Features.Animals.Commands.CreateAnimal;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Animal, AnimalDto>();
        CreateMap<CreateAnimalCommand, Animal>();
        CreateMap<ShelterProfile, ShelterDto>();
        CreateMap<AdoptionApplication, AdoptionApplicationDto>();
        CreateMap<LostFoundReport, LostFoundReportDto>();
        CreateMap<Donation, DonationDto>();
        CreateMap<DonationGoal, DonationGoalDto>();
        CreateMap<VolunteerApplication, VolunteerApplicationDto>();
        CreateMap<FosterProfile, FosterProfileDto>();
        CreateMap<Notification, NotificationDto>();
        CreateMap<AdopterProfile, AdopterProfileDto>();
        CreateMap<HealthRecord, HealthRecordDto>();
        CreateMap<BehaviorProfile, BehaviorProfileDto>();
        CreateMap<AdoptionContract, AdoptionContractDto>();
        CreateMap<PostAdoptionReport, PostAdoptionReportDto>();
        CreateMap<AnimalPhoto, MediaFileDto>();
        CreateMap<ShelterPhoto, MediaFileDto>();
        CreateMap<FileAsset, FileAssetDto>();
        CreateMap<SavedAnimalSearchFilter, SavedAnimalSearchFilterDto>();
        CreateMap<FosterAssignment, FosterAssignmentDto>();
        CreateMap<FosterReport, FosterReportDto>();
    }
}