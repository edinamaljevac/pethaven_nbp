using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
namespace PetHaven.Application.Features.Media.Commands.AddAnimalPhoto;
public class AddAnimalPhotoCommandHandler : IRequestHandler<AddAnimalPhotoCommand, MediaFileDto>
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public AddAnimalPhotoCommandHandler(IUnitOfWork uow,IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<MediaFileDto> Handle(AddAnimalPhotoCommand r, CancellationToken ct)
    {
        _ = await _uow.Animals.GetByIdAsync(r.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        if (r.IsMain) { foreach (var p in (await _uow.AnimalPhotos.GetAllAsync()).Where(x=>x.AnimalId==r.AnimalId && x.IsMain)) { p.IsMain=false; _uow.AnimalPhotos.Update(p); } }
        var photo = new AnimalPhoto { AnimalId = r.AnimalId, Url = r.Url.Trim(), IsMain = r.IsMain };
        await _uow.AnimalPhotos.AddAsync(photo); await _uow.SaveChangesAsync(); return _mapper.Map<MediaFileDto>(photo);
    }
}