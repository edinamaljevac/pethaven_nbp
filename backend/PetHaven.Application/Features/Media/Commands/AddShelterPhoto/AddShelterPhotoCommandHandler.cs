using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.Media.Commands.AddShelterPhoto;
public class AddShelterPhotoCommandHandler : IRequestHandler<AddShelterPhotoCommand, MediaFileDto>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public AddShelterPhotoCommandHandler(IUnitOfWork uow,IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<MediaFileDto> Handle(AddShelterPhotoCommand r, CancellationToken ct)
    {
        _ = await _uow.ShelterProfiles.GetByIdAsync(r.ShelterProfileId) ?? throw new KeyNotFoundException("Shelter not found.");
        var photo = new ShelterPhoto { ShelterProfileId = r.ShelterProfileId, Url = r.Url.Trim() };
        await _uow.ShelterPhotos.AddAsync(photo); await _uow.SaveChangesAsync(); return _mapper.Map<MediaFileDto>(photo);
    }
}