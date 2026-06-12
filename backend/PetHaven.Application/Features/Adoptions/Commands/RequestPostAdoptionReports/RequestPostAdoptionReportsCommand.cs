using MediatR;
namespace PetHaven.Application.Features.Adoptions.Commands.RequestPostAdoptionReports;
public class RequestPostAdoptionReportsCommand : IRequest
{
    public Guid ApplicationId { get; set; }
}