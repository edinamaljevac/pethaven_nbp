using Microsoft.EntityFrameworkCore;
using PetHaven.Domain.Entities;

namespace PetHaven.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Animal> Animals => Set<Animal>();

    public DbSet<AdopterProfile> AdopterProfiles => Set<AdopterProfile>();

    public DbSet<ShelterProfile> ShelterProfiles => Set<ShelterProfile>();

    public DbSet<FosterProfile> FosterProfiles => Set<FosterProfile>();

    public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();

    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();

    public DbSet<BehaviorProfile> BehaviorProfiles => Set<BehaviorProfile>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Donation> Donations => Set<Donation>();

    public DbSet<VolunteerApplication> VolunteerApplications => Set<VolunteerApplication>();
    public DbSet<AnimalPhoto> AnimalPhotos => Set<AnimalPhoto>();
    public DbSet<ShelterPhoto> ShelterPhotos => Set<ShelterPhoto>();
    public DbSet<LostFoundReport> LostFoundReports => Set<LostFoundReport>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DonationGoal> DonationGoals => Set<DonationGoal>();
    public DbSet<AdoptionContract> AdoptionContracts => Set<AdoptionContract>();
    public DbSet<PostAdoptionReport> PostAdoptionReports => Set<PostAdoptionReport>();
    public DbSet<SavedAnimalSearchFilter> SavedAnimalSearchFilters => Set<SavedAnimalSearchFilter>();
    public DbSet<FosterAssignment> FosterAssignments => Set<FosterAssignment>();
    public DbSet<FosterReport> FosterReports => Set<FosterReport>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<ContentModeration> ContentModerations => Set<ContentModeration>();
    public DbSet<LoginEvent> LoginEvents => Set<LoginEvent>();
}
