namespace PetHaven.Application.DTOs;

public class PlatformStatisticsDto
{
    public AdminStatisticsDto Totals { get; set; } = new();
    public int PendingShelters { get; set; }
    public int ActiveFosterPlacements { get; set; }
    public int ResolvedLostFoundReports { get; set; }
    public double AverageAdoptionDays { get; set; }
    public List<StatisticsPointDto> AnimalsByStatus { get; set; } = [];
    public List<StatisticsPointDto> ApplicationsByStatus { get; set; } = [];
    public List<StatisticsPointDto> AdoptionsByMonth { get; set; } = [];
    public List<StatisticsPointDto> DonationsByMonth { get; set; } = [];
}

public class StatisticsPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
