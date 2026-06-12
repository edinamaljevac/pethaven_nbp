using MediatR;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.Admin.Commands.ModerateContent;
public class ModerateContentCommand : IRequest
{
    public ModerationTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ModerationStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid ModeratorUserId { get; set; }
}