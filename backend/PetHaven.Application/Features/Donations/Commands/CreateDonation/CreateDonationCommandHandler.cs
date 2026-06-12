using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.Donations.Commands.CreateDonation;
public class CreateDonationCommandHandler : IRequestHandler<CreateDonationCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    public CreateDonationCommandHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<Guid> Handle(CreateDonationCommand r, CancellationToken ct)
    {
        var shelterProfileId = r.ShelterProfileId;
        var purpose = r.Purpose.Trim();
        if (r.DonationGoalId.HasValue)
        {
            var goal = await _uow.DonationGoals.GetByIdAsync(r.DonationGoalId.Value) ?? throw new KeyNotFoundException("Donation goal not found.");
            if (shelterProfileId == Guid.Empty)
            {
                shelterProfileId = goal.ShelterProfileId;
            }

            if (shelterProfileId != goal.ShelterProfileId)
            {
                throw new InvalidOperationException("Donation goal does not belong to the selected shelter.");
            }

            goal.CurrentAmount += r.Amount; goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount; goal.UpdatedAt = DateTime.UtcNow; _uow.DonationGoals.Update(goal);
            purpose = string.IsNullOrWhiteSpace(purpose) ? goal.Title : purpose;
        }

        if (shelterProfileId == Guid.Empty)
        {
            throw new InvalidOperationException("Shelter or donation goal is required.");
        }

        _ = await _uow.ShelterProfiles.GetByIdAsync(shelterProfileId) ?? throw new KeyNotFoundException("Shelter not found.");

        var donation = new Donation { ShelterProfileId = shelterProfileId, UserId = r.UserId, Amount = r.Amount, Purpose = purpose, IsPaid = true };
        await _uow.Donations.AddAsync(donation); await _uow.SaveChangesAsync(); return donation.Id;
    }
}
