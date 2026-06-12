using PetHaven.Application.DTOs;

namespace PetHaven.Application.Interfaces;

public interface IGeocodingService
{
    Task<GeocodingResultDto?> FindCoordinatesAsync(string location, CancellationToken cancellationToken);
}
