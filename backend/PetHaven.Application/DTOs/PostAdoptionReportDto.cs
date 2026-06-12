using PetHaven.Domain.Enums;
namespace PetHaven.Application.DTOs;
public class PostAdoptionReportDto
{
    public Guid Id { get; set; }
    public Guid AdoptionApplicationId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string AdopterName { get; set; } = string.Empty;
    public PostAdoptionReportType Type { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string ReportText { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsSubmitted { get; set; }
}
