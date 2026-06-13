namespace PetHaven.Api.Settings;

public class GeoLocationSettings
{
    public string DatabasePath { get; set; } = "GeoLite2-City.mmdb";

    /// <summary>
    /// Location used when the client IP cannot be resolved to a real location
    /// (e.g. loopback/private addresses in local development, or when the MaxMind
    /// database is unavailable). Lets "Shelters Near You" work without a public IP.
    /// </summary>
    public FallbackLocationSettings? FallbackLocation { get; set; }
}

public class FallbackLocationSettings
{
    public string Country { get; set; } = "Unknown";
    public string Region { get; set; } = "Unknown";
    public string City { get; set; } = "Unknown";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}