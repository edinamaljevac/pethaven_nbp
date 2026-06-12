using AutoMapper;
using MediatR;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Application.Features.Shelters.Queries.GetShelters;

public class GetSheltersQueryHandler : IRequestHandler<GetSheltersQuery, List<ShelterDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetSheltersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ShelterDto>> Handle(GetSheltersQuery request, CancellationToken cancellationToken)
    {
        var shelters = await _unitOfWork.ShelterProfiles.GetAllAsync();

        if (request.IsVerified.HasValue)
        {
            shelters = shelters.Where(x => x.IsVerified == request.IsVerified.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            shelters = shelters.Where(x => x.Location.Contains(request.Location, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (request.Latitude.HasValue && request.Longitude.HasValue && request.RadiusKm.HasValue)
        {
            shelters = shelters
                .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
                .Where(x => DistanceKm(request.Latitude.Value, request.Longitude.Value, x.Latitude!.Value, x.Longitude!.Value) <= request.RadiusKm.Value)
                .ToList();
        }

        return _mapper.Map<List<ShelterDto>>(shelters);
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}