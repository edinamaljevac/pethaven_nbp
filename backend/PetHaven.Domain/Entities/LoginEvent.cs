using PetHaven.Domain.Common;

namespace PetHaven.Domain.Entities;

public class LoginEvent : BaseEntity
{
    public Guid UserId { get; set; }

    public string Country { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
