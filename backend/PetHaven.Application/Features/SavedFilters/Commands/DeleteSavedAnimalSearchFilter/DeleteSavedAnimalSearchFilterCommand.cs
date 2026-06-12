using MediatR;

namespace PetHaven.Application.Features.SavedFilters.Commands.DeleteSavedAnimalSearchFilter;

public class DeleteSavedAnimalSearchFilterCommand : IRequest
{
    public Guid FilterId { get; set; }
    public Guid UserId { get; set; }
}
