namespace PetHaven.Application.DTOs;

public class FosterProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FosterName { get; set; } = string.Empty;
    public string FosterEmail { get; set; } = string.Empty;
    public string PreferredAnimalType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int ActiveAssignments { get; set; }
    public int AvailableCapacity => Math.Max(0, Capacity - ActiveAssignments);
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
}
