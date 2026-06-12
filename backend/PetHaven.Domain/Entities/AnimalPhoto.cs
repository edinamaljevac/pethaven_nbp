using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class AnimalPhoto : BaseEntity
{
    public Guid AnimalId { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool IsMain { get; set; }

    public Animal Animal { get; set; } = null!;
}