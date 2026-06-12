using PetHaven.Domain.Common;
using PetHaven.Domain.Enums;

namespace PetHaven.Domain.Entities;

public class ContentModeration : BaseEntity
{
    public ModerationTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ModerationStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid ModeratorUserId { get; set; }
    public User ModeratorUser { get; set; } = null!;
}