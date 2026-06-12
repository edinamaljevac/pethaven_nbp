using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
namespace PetHaven.Application.Features.SavedFilters.Queries.GetSavedAnimalSearchFilters;
public class GetSavedAnimalSearchFiltersQueryHandler : IRequestHandler<GetSavedAnimalSearchFiltersQuery, List<SavedAnimalSearchFilterDto>>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _uow;
    public GetSavedAnimalSearchFiltersQueryHandler(IUnitOfWork uow,IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<List<SavedAnimalSearchFilterDto>> Handle(GetSavedAnimalSearchFiltersQuery r, CancellationToken ct)=>_mapper.Map<List<SavedAnimalSearchFilterDto>>((await _uow.SavedAnimalSearchFilters.GetAllAsync()).Where(x=>x.UserId==r.UserId).ToList());
}