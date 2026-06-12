using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Persistence.Context;
using PetHaven.Persistence.Repositories;

namespace PetHaven.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Animals = new Repository<Animal>(_context);
        AdopterProfiles = new Repository<AdopterProfile>(_context);
        ShelterProfiles = new Repository<ShelterProfile>(_context);
        FosterProfiles = new Repository<FosterProfile>(_context);
        AdoptionApplications = new Repository<AdoptionApplication>(_context);
        HealthRecords = new Repository<HealthRecord>(_context);
        BehaviorProfiles = new Repository<BehaviorProfile>(_context);
        Donations = new Repository<Donation>(_context);
        VolunteerApplications = new Repository<VolunteerApplication>(_context);
        RefreshTokens = new Repository<RefreshToken>(_context);
        AnimalPhotos = new Repository<AnimalPhoto>(_context);
        ShelterPhotos = new Repository<ShelterPhoto>(_context);
        LostFoundReports = new Repository<LostFoundReport>(_context);
        Notifications = new Repository<Notification>(_context);
        DonationGoals = new Repository<DonationGoal>(_context);
        AdoptionContracts = new Repository<AdoptionContract>(_context);
        PostAdoptionReports = new Repository<PostAdoptionReport>(_context);
        SavedAnimalSearchFilters = new Repository<SavedAnimalSearchFilter>(_context);
        FosterAssignments = new Repository<FosterAssignment>(_context);
        FosterReports = new Repository<FosterReport>(_context);
        FileAssets = new Repository<FileAsset>(_context);
        ContentModerations = new Repository<ContentModeration>(_context);
        LoginEvents = new Repository<LoginEvent>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Animal> Animals { get; }
    public IRepository<AdopterProfile> AdopterProfiles { get; }
    public IRepository<ShelterProfile> ShelterProfiles { get; }
    public IRepository<FosterProfile> FosterProfiles { get; }
    public IRepository<AdoptionApplication> AdoptionApplications { get; }
    public IRepository<HealthRecord> HealthRecords { get; }
    public IRepository<BehaviorProfile> BehaviorProfiles { get; }
    public IRepository<Donation> Donations { get; }
    public IRepository<VolunteerApplication> VolunteerApplications { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }
    public IRepository<AnimalPhoto> AnimalPhotos { get; }
    public IRepository<ShelterPhoto> ShelterPhotos { get; }
    public IRepository<LostFoundReport> LostFoundReports { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<DonationGoal> DonationGoals { get; }
    public IRepository<AdoptionContract> AdoptionContracts { get; }
    public IRepository<PostAdoptionReport> PostAdoptionReports { get; }
    public IRepository<SavedAnimalSearchFilter> SavedAnimalSearchFilters { get; }
    public IRepository<FosterAssignment> FosterAssignments { get; }
    public IRepository<FosterReport> FosterReports { get; }
    public IRepository<FileAsset> FileAssets { get; }
    public IRepository<ContentModeration> ContentModerations { get; }
    public IRepository<LoginEvent> LoginEvents { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
