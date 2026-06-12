namespace PetHaven.Application.DTOs;
public class MediaFileDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}