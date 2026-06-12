namespace PetHaven.Api.Middleware;

public sealed record RequestGeo(
    string Country,
    string Region,
    string City,
    string IpAddress,
    double? Latitude,
    double? Longitude);
