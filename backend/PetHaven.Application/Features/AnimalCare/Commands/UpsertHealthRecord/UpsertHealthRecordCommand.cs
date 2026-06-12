using MediatR;
using PetHaven.Application.DTOs;

namespace PetHaven.Application.Features.AnimalCare.Commands.UpsertHealthRecord;

public class UpsertHealthRecordCommand : IRequest<HealthRecordDto>
{
    public Guid AnimalId { get; set; }
    public bool IsVaccinated { get; set; }
    public bool IsSterilized { get; set; }
    public bool IsMicrochipped { get; set; }
    public string ChronicDiseases { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
}