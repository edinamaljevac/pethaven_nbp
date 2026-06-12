using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Media.Commands.AddShelterPhoto;
public class AddShelterPhotoCommand : IRequest<MediaFileDto>
{
    public Guid ShelterProfileId { get; set; }
    public string Url { get; set; } = string.Empty;
}