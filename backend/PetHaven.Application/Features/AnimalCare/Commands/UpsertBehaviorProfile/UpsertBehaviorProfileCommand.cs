using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.AnimalCare.Commands.UpsertBehaviorProfile;

public class UpsertBehaviorProfileCommand : IRequest<BehaviorProfileDto>
{
    public Guid AnimalId { get; set; }
    public EnergyLevel EnergyLevel { get; set; }
    public bool GoodWithChildren { get; set; }
    public bool GoodWithDogs { get; set; }
    public bool GoodWithCats { get; set; }
    public bool HasSpecialNeeds { get; set; }
    public string BehaviorDescription { get; set; } = string.Empty;
}