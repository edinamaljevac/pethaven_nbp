using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class AdoptionContract : BaseEntity
{
    public Guid AdoptionApplicationId { get; set; }

    public string PdfUrl { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public AdoptionApplication AdoptionApplication { get; set; } = null!;
}