using PetHaven.Domain.Entities;

namespace PetHaven.Application.Interfaces;
public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Animal> Animals { get; }
    IRepository<AdopterProfile> AdopterProfiles { get; }
    IRepository<ShelterProfile> ShelterProfiles { get; }
    IRepository<FosterProfile> FosterProfiles { get; }
    IRepository<AdoptionApplication> AdoptionApplications { get; }
    IRepository<HealthRecord> HealthRecords { get; }
    IRepository<BehaviorProfile> BehaviorProfiles { get; }
    IRepository<Donation> Donations { get; }
    IRepository<VolunteerApplication> VolunteerApplications { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<AnimalPhoto> AnimalPhotos { get; }
    IRepository<ShelterPhoto> ShelterPhotos { get; }
    IRepository<LostFoundReport> LostFoundReports { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<DonationGoal> DonationGoals { get; }
    IRepository<AdoptionContract> AdoptionContracts { get; }
    IRepository<PostAdoptionReport> PostAdoptionReports { get; }
    IRepository<SavedAnimalSearchFilter> SavedAnimalSearchFilters { get; }
    IRepository<FosterAssignment> FosterAssignments { get; }
    IRepository<FosterReport> FosterReports { get; }
    IRepository<FileAsset> FileAssets { get; }
    IRepository<ContentModeration> ContentModerations { get; }
    IRepository<LoginEvent> LoginEvents { get; }

    Task<int> SaveChangesAsync();
}
