using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
namespace PetHaven.Application.Features.LostFound.Queries.FindLostFoundMatches;
public class FindLostFoundMatchesQueryHandler : IRequestHandler<FindLostFoundMatchesQuery, List<LostFoundMatchDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public FindLostFoundMatchesQueryHandler(IUnitOfWork uow,IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<List<LostFoundMatchDto>> Handle(FindLostFoundMatchesQuery r, CancellationToken ct)
    {
        var source = await _uow.LostFoundReports.GetByIdAsync(r.ReportId) ?? throw new KeyNotFoundException("Lost/found report not found.");
        if (!r.IsShelter && source.UserId != r.CurrentUserId) throw new UnauthorizedAccessException("You can view matches only for your own reports.");
        if (source.IsResolved) throw new InvalidOperationException("Matches are available only for active reports.");

        var candidates = (await _uow.LostFoundReports.GetAllAsync()).Where(x => x.Id != source.Id && x.Type != source.Type && !x.IsResolved && !x.IsHidden).ToList();
        var matched = candidates.Select(x => new { Report = x, Score = Score(source.Species,x.Species)*35 + Score(source.Breed,x.Breed)*15 + Score(source.Color,x.Color)*20 + Score(source.Location,x.Location)*20 + Score(source.Description,x.Description)*10 })
            .Where(x => x.Score >= 35).OrderByDescending(x => x.Score)
            .ToList();
        var users = (await _uow.Users.GetAllAsync()).Where(x => matched.Select(match => match.Report.UserId).Contains(x.Id)).ToDictionary(x => x.Id);
        return matched.Select(x =>
        {
            var report = _mapper.Map<LostFoundReportDto>(x.Report);
            if (users.TryGetValue(report.UserId, out var user)) report.ReporterEmail = user.Email;
            return new LostFoundMatchDto { Report = report, Score = x.Score, Reason = "Potential match by species, color, location and description." };
        }).ToList();
    }
    private static int Score(string a,string b) => string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b) ? 0 : (a.Contains(b, StringComparison.OrdinalIgnoreCase) || b.Contains(a, StringComparison.OrdinalIgnoreCase) ? 1 : 0);
}
