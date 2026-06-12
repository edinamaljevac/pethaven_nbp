using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.AnimalCare.Commands.UpsertBehaviorProfile;

public class UpsertBehaviorProfileCommandHandler : IRequestHandler<UpsertBehaviorProfileCommand, BehaviorProfileDto>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _unitOfWork;
    public UpsertBehaviorProfileCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) { _unitOfWork = unitOfWork; _mapper = mapper; }
    public async Task<BehaviorProfileDto> Handle(UpsertBehaviorProfileCommand request, CancellationToken cancellationToken)
    {
        _ = await _unitOfWork.Animals.GetByIdAsync(request.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        var profile = await _unitOfWork.BehaviorProfiles.FirstOrDefaultAsync(x => x.AnimalId == request.AnimalId);
        if (profile is null) { profile = new BehaviorProfile { AnimalId = request.AnimalId }; await _unitOfWork.BehaviorProfiles.AddAsync(profile); }
        profile.EnergyLevel = request.EnergyLevel; profile.GoodWithChildren = request.GoodWithChildren; profile.GoodWithDogs = request.GoodWithDogs; profile.GoodWithCats = request.GoodWithCats; profile.HasSpecialNeeds = request.HasSpecialNeeds; profile.BehaviorDescription = request.BehaviorDescription.Trim(); profile.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<BehaviorProfileDto>(profile);
    }
}