using PetHaven.Domain.Enums;

namespace PetHaven.Application.DTOs;

public class LostFoundReportDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public LostFoundReportType Type { get; set; }
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public bool IsHidden { get; set; }
}
