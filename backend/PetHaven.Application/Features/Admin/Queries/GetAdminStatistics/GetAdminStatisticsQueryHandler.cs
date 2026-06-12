using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Enums;
namespace PetHaven.Application.Features.Admin.Queries.GetAdminStatistics;
public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IUnitOfWork _uow;
    public GetAdminStatisticsQueryHandler(IUnitOfWork uow)=>_uow=uow;
    public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery r, CancellationToken ct)
    {
        var users=await _uow.Users.GetAllAsync(); var shelters=await _uow.ShelterProfiles.GetAllAsync(); var animals=await _uow.Animals.GetAllAsync(); var apps=await _uow.AdoptionApplications.GetAllAsync(); var reports=await _uow.LostFoundReports.GetAllAsync(); var donations=await _uow.Donations.GetAllAsync();
        return new AdminStatisticsDto { Users=users.Count, Shelters=shelters.Count, VerifiedShelters=shelters.Count(x=>x.IsVerified), Animals=animals.Count, AvailableAnimals=animals.Count(x=>x.Status==AnimalStatus.Available), AdoptionApplications=apps.Count, AdoptedAnimals=animals.Count(x=>x.Status==AnimalStatus.Adopted), LostFoundOpenReports=reports.Count(x=>!x.IsResolved), PaidDonationTotal=donations.Where(x=>x.IsPaid).Sum(x=>x.Amount) };
    }
}