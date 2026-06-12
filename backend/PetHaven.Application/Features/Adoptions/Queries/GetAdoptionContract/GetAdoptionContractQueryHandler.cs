using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Adoptions.Queries.GetAdoptionContract;

public class GetAdoptionContractQueryHandler : IRequestHandler<GetAdoptionContractQuery, AdoptionContractDto>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public GetAdoptionContractQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<AdoptionContractDto> Handle(GetAdoptionContractQuery request, CancellationToken cancellationToken)
    {
        var contract = await _uow.AdoptionContracts.FirstOrDefaultAsync(x => x.AdoptionApplicationId == request.ApplicationId)
            ?? throw new KeyNotFoundException("Contract has not been generated yet.");

        return _mapper.Map<AdoptionContractDto>(contract);
    }
}
