using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.SavedFilters.Queries.GetSavedAnimalSearchFilters;
public class GetSavedAnimalSearchFiltersQuery : IRequest<List<SavedAnimalSearchFilterDto>>
{
    public Guid UserId { get; set; }
}