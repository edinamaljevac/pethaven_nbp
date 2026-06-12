using MediatR;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.Admin.Commands.ModerateContent;
public class ModerateContentCommandHandler : IRequestHandler<ModerateContentCommand>
{
    private readonly IUnitOfWork _uow;
    public ModerateContentCommandHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task Handle(ModerateContentCommand r, CancellationToken ct)
    {
        var moderation=new ContentModeration{TargetType=r.TargetType,TargetId=r.TargetId,Status=r.Status,Reason=r.Reason.Trim(),ModeratorUserId=r.ModeratorUserId};
        await _uow.ContentModerations.AddAsync(moderation); await _uow.SaveChangesAsync();
    }
}