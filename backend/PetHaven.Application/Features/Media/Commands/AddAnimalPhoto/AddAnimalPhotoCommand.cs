using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Media.Commands.AddAnimalPhoto;
public class AddAnimalPhotoCommand : IRequest<MediaFileDto>
{
    public Guid AnimalId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}