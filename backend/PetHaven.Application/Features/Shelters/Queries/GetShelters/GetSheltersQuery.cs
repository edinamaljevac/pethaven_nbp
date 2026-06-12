using MediatR;
using PetHaven.Application.DTOs;
namespace PetHaven.Application.Features.Shelters.Queries.GetShelters;
public class GetSheltersQuery : IRequest<List<ShelterDto>>
{
    public bool? IsVerified { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
}