using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Features.Fosters.Queries.GetFosterAssignments;
using PetHaven.Application.Features.Notifications.Events;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Fosters.Commands.EndFosterAssignment;

public class EndFosterAssignmentCommandHandler : IRequestHandler<EndFosterAssignmentCommand, FosterAssignmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public EndFosterAssignmentCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<FosterAssignmentDto> Handle(EndFosterAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.FosterAssignments.GetByIdAsync(request.FosterAssignmentId)
            ?? throw new KeyNotFoundException("Foster assignment not found.");
        if (!assignment.IsActive)
        {
            throw new InvalidOperationException("Foster placement is already completed.");
        }

        var animal = await _unitOfWork.Animals.GetByIdAsync(assignment.AnimalId)
            ?? throw new KeyNotFoundException("Animal not found.");

        assignment.IsActive = false;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        if (animal.Status != AnimalStatus.Adopted)
        {
            animal.Status = AnimalStatus.Available;
            animal.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Animals.Update(animal);
        }

        _unitOfWork.FosterAssignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        if (animal.Status == AnimalStatus.Available)
        {
            await _mediator.Publish(new AnimalBecameAvailableNotification(animal.Id), cancellationToken);
        }

        var results = await new GetFosterAssignmentsQueryHandler(_unitOfWork).Handle(new GetFosterAssignmentsQuery { FosterProfileId = assignment.FosterProfileId }, cancellationToken);
        return results.First(x => x.Id == assignment.Id);
    }
}
