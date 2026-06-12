using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class ShelterPhoto : BaseEntity
{
    public Guid ShelterProfileId { get; set; }

    public string Url { get; set; } = string.Empty;

    public ShelterProfile ShelterProfile { get; set; } = null!;
}