using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class HealthRecord : BaseEntity
{
    public Guid AnimalId { get; set; }

    public bool IsVaccinated { get; set; }

    public bool IsSterilized { get; set; }

    public bool IsMicrochipped { get; set; }

    public string ChronicDiseases { get; set; } = string.Empty;

    public string Medications { get; set; } = string.Empty;

    public Animal Animal { get; set; } = null!;
}