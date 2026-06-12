using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.LostFound.Queries.SearchLostFoundReports;

public class SearchLostFoundReportsQueryHandler : IRequestHandler<SearchLostFoundReportsQuery, List<LostFoundReportDto>>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _unitOfWork;
    public SearchLostFoundReportsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) { _unitOfWork = unitOfWork; _mapper = mapper; }
    public async Task<List<LostFoundReportDto>> Handle(SearchLostFoundReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _unitOfWork.LostFoundReports.GetAllAsync();
        if (!request.IsAdmin) reports = reports.Where(x => !x.IsHidden).ToList();
        if (request.OwnReportsOnly && request.CurrentUserId.HasValue) reports = reports.Where(x => x.UserId == request.CurrentUserId.Value).ToList();
        if (!request.IncludeResolved) reports = reports.Where(x => !x.IsResolved).ToList();
        if (request.Type.HasValue) reports = reports.Where(x => x.Type == request.Type.Value).ToList();
        if (!string.IsNullOrWhiteSpace(request.Species)) reports = reports.Where(x => x.Species.Contains(request.Species, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(request.Location)) reports = reports.Where(x => x.Location.Contains(request.Location, StringComparison.OrdinalIgnoreCase)).ToList();
        var ordered = reports.OrderByDescending(x => x.ReportDate).ToList();
        var result = _mapper.Map<List<LostFoundReportDto>>(ordered);
        foreach (var report in result)
        {
            report.ReporterEmail = string.Empty;
            report.ContactPhone = string.Empty;
        }
        return result;
    }
}
