using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PetHaven.Application.DTOs;
using PetHaven.Application.Interfaces;

namespace PetHaven.Infrastructure.Services;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public NominatimGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeocodingResultDto?> FindCoordinatesAsync(string location, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var path = $"search?format=jsonv2&limit=1&q={Uri.EscapeDataString(location.Trim())}";
        var results = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(path, cancellationToken);
        var result = results?.FirstOrDefault();

        if (result is null
            || !double.TryParse(result.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !double.TryParse(result.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return null;
        }

        return new GeocodingResultDto
        {
            Latitude = latitude,
            Longitude = longitude,
            DisplayName = result.DisplayName
        };
    }

    private sealed class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; set; } = string.Empty;

        [JsonPropertyName("lon")]
        public string Longitude { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
    }
}
