using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.LostFound.Queries.SearchLostFoundReports;

public class SearchLostFoundReportsQuery : IRequest<List<LostFoundReportDto>>
{
    public LostFoundReportType? Type { get; set; }
    public string? Species { get; set; }
    public string? Location { get; set; }
    public bool IncludeResolved { get; set; }
    public Guid? CurrentUserId { get; set; }
    public bool OwnReportsOnly { get; set; }
    public bool IsAdmin { get; set; }
}
