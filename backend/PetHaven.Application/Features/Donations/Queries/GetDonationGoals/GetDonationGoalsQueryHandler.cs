using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
namespace PetHaven.Application.Features.Donations.Queries.GetDonationGoals;
public class GetDonationGoalsQueryHandler : IRequestHandler<GetDonationGoalsQuery, List<DonationGoalDto>>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public GetDonationGoalsQueryHandler(IUnitOfWork uow, IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<List<DonationGoalDto>> Handle(GetDonationGoalsQuery r, CancellationToken ct)
    {
        var goals = await _uow.DonationGoals.GetAllAsync();
        if (r.ShelterProfileId.HasValue) goals = goals.Where(x => x.ShelterProfileId == r.ShelterProfileId.Value).ToList();
        if (r.ActiveOnly) goals = goals.Where(x => !x.IsCompleted).ToList();
        return _mapper.Map<List<DonationGoalDto>>(goals);
    }
}