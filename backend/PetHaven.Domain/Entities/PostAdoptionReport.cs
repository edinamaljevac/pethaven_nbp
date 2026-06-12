using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class PostAdoptionReport : BaseEntity
{
    public Guid AdoptionApplicationId { get; set; }

    public PostAdoptionReportType Type { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string ReportText { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public bool IsSubmitted { get; set; }

    public AdoptionApplication AdoptionApplication { get; set; } = null!;
}