using MediatR;
using PetHaven.Application.Features.Notifications.Events;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Commands.UpdateAdoptionStatus;

public class UpdateAdoptionStatusCommandHandler : IRequestHandler<UpdateAdoptionStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public UpdateAdoptionStatusCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task Handle(UpdateAdoptionStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.AdoptionApplications.GetByIdAsync(request.ApplicationId)
            ?? throw new KeyNotFoundException("Adoption application not found.");

        if (!IsAllowedTransition(application.Status, request.Status))
        {
            throw new InvalidOperationException(
                $"Adoption application cannot move from {application.Status} to {request.Status}.");
        }

        if (request.Status is AdoptionApplicationStatus.InterviewScheduled or AdoptionApplicationStatus.HomeVisitScheduled)
        {
            if (!request.ScheduledAt.HasValue) throw new InvalidOperationException("Please choose a date and time for the scheduled appointment.");
            if (request.ScheduledAt.Value <= DateTime.UtcNow) throw new InvalidOperationException("The scheduled appointment must be in the future.");
        }

        application.Status = request.Status;
        application.Notes = request.Notes.Trim();
        if (request.Status == AdoptionApplicationStatus.InterviewScheduled) application.InterviewScheduledAt = request.ScheduledAt;
        if (request.Status == AdoptionApplicationStatus.HomeVisitScheduled) application.HomeVisitScheduledAt = request.ScheduledAt;
        application.UpdatedAt = DateTime.UtcNow;

        var animal = await _unitOfWork.Animals.GetByIdAsync(application.AnimalId);
        if (animal is not null)
        {
            if (request.Status == AdoptionApplicationStatus.Adopted)
            {
                animal.Status = AnimalStatus.Adopted;
            }
            else if (request.Status == AdoptionApplicationStatus.Rejected)
            {
                animal.Status = AnimalStatus.Available;
            }

            _unitOfWork.Animals.Update(animal);
        }

        var adopterProfile = await _unitOfWork.AdopterProfiles.GetByIdAsync(application.AdopterProfileId);
        if (adopterProfile is not null)
        {
            var appointment = request.Status == AdoptionApplicationStatus.InterviewScheduled
                ? application.InterviewScheduledAt
                : request.Status == AdoptionApplicationStatus.HomeVisitScheduled ? application.HomeVisitScheduledAt : null;
            var appointmentText = appointment.HasValue ? $" Scheduled for {appointment.Value:dd/MM/yyyy HH:mm} UTC." : string.Empty;
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = adopterProfile.UserId,
                Type = NotificationType.AdoptionStatusChanged,
                Title = "Adoption application status changed",
                Message = $"Your adoption application for {animal?.Name ?? "an animal"} is now {request.Status}.{appointmentText}"
            });
        }

        _unitOfWork.AdoptionApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        if (animal?.Status == AnimalStatus.Available)
        {
            await _mediator.Publish(new AnimalBecameAvailableNotification(animal.Id), cancellationToken);
        }
    }

    private static bool IsAllowedTransition(
        AdoptionApplicationStatus current,
        AdoptionApplicationStatus next)
    {
        return (current, next) switch
        {
            (AdoptionApplicationStatus.Submitted, AdoptionApplicationStatus.UnderReview) => true,
            (AdoptionApplicationStatus.UnderReview, AdoptionApplicationStatus.InterviewScheduled) => true,
            (AdoptionApplicationStatus.InterviewScheduled, AdoptionApplicationStatus.HomeVisitScheduled) => true,
            (AdoptionApplicationStatus.HomeVisitScheduled, AdoptionApplicationStatus.Approved) => true,
            (AdoptionApplicationStatus.HomeVisitScheduled, AdoptionApplicationStatus.Rejected) => true,
            (AdoptionApplicationStatus.Approved, AdoptionApplicationStatus.Adopted) => true,
            _ => false
        };
    }
}
