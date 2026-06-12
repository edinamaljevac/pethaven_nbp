using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Application.Features.AnimalCare.Commands.UpsertHealthRecord;

public class UpsertHealthRecordCommandHandler : IRequestHandler<UpsertHealthRecordCommand, HealthRecordDto>
{
    private readonly IMapper _mapper; private readonly IUnitOfWork _unitOfWork;
    public UpsertHealthRecordCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) { _unitOfWork = unitOfWork; _mapper = mapper; }
    public async Task<HealthRecordDto> Handle(UpsertHealthRecordCommand request, CancellationToken cancellationToken)
    {
        _ = await _unitOfWork.Animals.GetByIdAsync(request.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        var record = await _unitOfWork.HealthRecords.FirstOrDefaultAsync(x => x.AnimalId == request.AnimalId);
        if (record is null) { record = new HealthRecord { AnimalId = request.AnimalId }; await _unitOfWork.HealthRecords.AddAsync(record); }
        record.IsVaccinated = request.IsVaccinated; record.IsSterilized = request.IsSterilized; record.IsMicrochipped = request.IsMicrochipped; record.ChronicDiseases = request.ChronicDiseases.Trim(); record.Medications = request.Medications.Trim(); record.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<HealthRecordDto>(record);
    }
}