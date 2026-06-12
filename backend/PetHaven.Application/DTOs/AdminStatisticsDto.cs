namespace PetHaven.Application.DTOs;
public class AdminStatisticsDto
{
    public int Users { get; set; }
    public int Shelters { get; set; }
    public int VerifiedShelters { get; set; }
    public int Animals { get; set; }
    public int AvailableAnimals { get; set; }
    public int AdoptionApplications { get; set; }
    public int AdoptedAnimals { get; set; }
    public int LostFoundOpenReports { get; set; }
    public decimal PaidDonationTotal { get; set; }
}