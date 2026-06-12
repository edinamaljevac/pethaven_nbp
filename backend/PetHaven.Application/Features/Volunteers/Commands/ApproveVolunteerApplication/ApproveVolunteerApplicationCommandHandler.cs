using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Volunteers.Commands.ApproveVolunteerApplication;

public class ApproveVolunteerApplicationCommandHandler : IRequestHandler<ApproveVolunteerApplicationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveVolunteerApplicationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApproveVolunteerApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _unitOfWork.VolunteerApplications.GetByIdAsync(request.ApplicationId)
            ?? throw new KeyNotFoundException("Volunteer application not found.");

        if (application.Status != VolunteerApplicationStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted volunteer applications can be approved or rejected.");
        }

        application.IsApproved = request.IsApproved;
        application.Status = request.IsApproved ? VolunteerApplicationStatus.Approved : VolunteerApplicationStatus.Rejected;
        application.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.VolunteerApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();
    }
}
