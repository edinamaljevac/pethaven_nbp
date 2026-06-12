using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Donations.Queries.GetDonations;

public class GetDonationsQueryHandler : IRequestHandler<GetDonationsQuery, List<DonationDto>>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _unitOfWork;
    public GetDonationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) { _unitOfWork = unitOfWork; _mapper = mapper; }
    public async Task<List<DonationDto>> Handle(GetDonationsQuery request, CancellationToken cancellationToken)
    {
        var donations = await _unitOfWork.Donations.GetAllAsync();
        if (request.ShelterProfileId.HasValue) donations = donations.Where(x => x.ShelterProfileId == request.ShelterProfileId.Value).ToList();
        return _mapper.Map<List<DonationDto>>(donations);
    }
}