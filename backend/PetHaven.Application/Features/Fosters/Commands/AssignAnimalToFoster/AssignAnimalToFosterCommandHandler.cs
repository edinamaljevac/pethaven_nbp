using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.Fosters.Commands.AssignAnimalToFoster;
public class AssignAnimalToFosterCommandHandler : IRequestHandler<AssignAnimalToFosterCommand, FosterAssignmentDto>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _uow;
    public AssignAnimalToFosterCommandHandler(IUnitOfWork uow,IMapper mapper){_uow=uow;_mapper=mapper;}
    public async Task<FosterAssignmentDto> Handle(AssignAnimalToFosterCommand r, CancellationToken ct)
    {
        var animal=await _uow.Animals.GetByIdAsync(r.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        if (animal.Status != AnimalStatus.Available) throw new InvalidOperationException("Only available animals can be assigned to foster care.");
        var fosterProfile=await _uow.FosterProfiles.GetByIdAsync(r.FosterProfileId) ?? throw new KeyNotFoundException("Foster profile not found.");
        var activeAssignments=(await _uow.FosterAssignments.GetAllAsync()).Count(x => x.FosterProfileId == r.FosterProfileId && x.IsActive);
        if (activeAssignments >= fosterProfile.Capacity) throw new InvalidOperationException("This foster profile has reached its capacity.");
        var assignment=new FosterAssignment{AnimalId=r.AnimalId,FosterProfileId=r.FosterProfileId,StartDate=r.StartDate,Notes=r.Notes.Trim()};
        animal.Status=AnimalStatus.InFosterCare; animal.UpdatedAt=DateTime.UtcNow;
        await _uow.FosterAssignments.AddAsync(assignment); _uow.Animals.Update(animal); await _uow.SaveChangesAsync(); return _mapper.Map<FosterAssignmentDto>(assignment);
    }
}
