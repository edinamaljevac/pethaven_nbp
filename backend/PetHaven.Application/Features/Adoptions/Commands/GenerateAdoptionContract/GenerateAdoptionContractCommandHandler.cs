using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;
using PetHaven.Domain.Enums;

namespace PetHaven.Application.Features.Adoptions.Commands.GenerateAdoptionContract;

public class GenerateAdoptionContractCommandHandler : IRequestHandler<GenerateAdoptionContractCommand, AdoptionContractDto>
{
    private readonly IAdoptionContractGenerator _contractGenerator;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public GenerateAdoptionContractCommandHandler(IUnitOfWork uow, IMapper mapper, IAdoptionContractGenerator contractGenerator)
    {
        _uow = uow;
        _mapper = mapper;
        _contractGenerator = contractGenerator;
    }

    public async Task<AdoptionContractDto> Handle(GenerateAdoptionContractCommand request, CancellationToken cancellationToken)
    {
        var application = await _uow.AdoptionApplications.GetByIdAsync(request.ApplicationId) ?? throw new KeyNotFoundException("Adoption application not found.");
        if (application.Status is not AdoptionApplicationStatus.Approved and not AdoptionApplicationStatus.Adopted)
        {
            throw new InvalidOperationException("An adoption contract can be generated only after the application is approved.");
        }

        var animal = await _uow.Animals.GetByIdAsync(application.AnimalId) ?? throw new KeyNotFoundException("Animal not found.");
        var existing = await _uow.AdoptionContracts.FirstOrDefaultAsync(x => x.AdoptionApplicationId == request.ApplicationId);
        if (existing is not null) return _mapper.Map<AdoptionContractDto>(existing);

        var pdfUrl = await _contractGenerator.GenerateAsync(application, animal, cancellationToken);
        var contract = new AdoptionContract { AdoptionApplicationId = request.ApplicationId, PdfUrl = pdfUrl, GeneratedAt = DateTime.UtcNow };
        await _uow.AdoptionContracts.AddAsync(contract);
        await _uow.SaveChangesAsync();
        return _mapper.Map<AdoptionContractDto>(contract);
    }
}
