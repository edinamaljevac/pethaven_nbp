using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.Volunteers.Commands.SubmitVolunteerApplication;

public class SubmitVolunteerApplicationCommandHandler : IRequestHandler<SubmitVolunteerApplicationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public SubmitVolunteerApplicationCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<Guid> Handle(SubmitVolunteerApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = new VolunteerApplication { UserId = request.UserId, ShelterProfileId = request.ShelterProfileId, PreferredActivities = request.PreferredActivities.Trim(), Availability = request.Availability.Trim(), Motivation = request.Motivation.Trim() };
        await _unitOfWork.VolunteerApplications.AddAsync(application); await _unitOfWork.SaveChangesAsync(); return application.Id;
    }
}