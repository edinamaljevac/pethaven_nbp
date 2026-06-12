using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.Donations.Commands.CreateDonationGoal;
public class CreateDonationGoalCommandHandler : IRequestHandler<CreateDonationGoalCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    public CreateDonationGoalCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<Guid> Handle(CreateDonationGoalCommand r, CancellationToken ct)
    {
        _ = await _uow.ShelterProfiles.GetByIdAsync(r.ShelterProfileId) ?? throw new KeyNotFoundException("Shelter not found.");
        var goal = new DonationGoal { ShelterProfileId = r.ShelterProfileId, Title = r.Title.Trim(), Description = r.Description.Trim(), TargetAmount = r.TargetAmount };
        await _uow.DonationGoals.AddAsync(goal); await _uow.SaveChangesAsync(); return goal.Id;
    }
}