namespace PetHaven.Application.DTOs;
public class AdoptionContractDto
{
    public Guid Id { get; set; }
    public Guid AdoptionApplicationId { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}