namespace PetHaven.Application.DTOs;
public class LostFoundMatchDto
{
    public LostFoundReportDto Report { get; set; } = new();
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}