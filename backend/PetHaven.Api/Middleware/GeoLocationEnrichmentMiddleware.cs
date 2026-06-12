using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.Extensions.Options;
using PetHaven.Api.Settings;
using System.Net;

namespace PetHaven.Api.Middleware;

public class GeoLocationEnrichmentMiddleware
{
    public const string ContextKey = "RequestGeo";
    private readonly GeoLocationSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<GeoLocationEnrichmentMiddleware> _logger;
    private readonly RequestDelegate _next;

    public GeoLocationEnrichmentMiddleware(
        RequestDelegate next,
        IOptions<GeoLocationSettings> settings,
        IWebHostEnvironment environment,
        ILogger<GeoLocationEnrichmentMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = ResolveClientIp(context);
        context.Items[ContextKey] = ResolveGeo(context, ipAddress);
        await _next(context);
    }

    private RequestGeo ResolveGeo(HttpContext context, string ipAddress)
    {
        var databasePath = ResolveDatabasePath();

        if (File.Exists(databasePath) && IPAddress.TryParse(ipAddress, out var parsedIp))
        {
            try
            {
                using var reader = new DatabaseReader(databasePath);
                var city = reader.City(parsedIp);

                return new RequestGeo(
                    city.Country.Name ?? "Unknown",
                    city.MostSpecificSubdivision.Name ?? "Unknown",
                    city.City.Name ?? "Unknown",
                    ipAddress,
                    city.Location.Latitude,
                    city.Location.Longitude);
            }
            catch (AddressNotFoundException)
            {
                return Unknown(ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not resolve geolocation for IP {IpAddress} using MaxMind database.", ipAddress);
            }
        }

        return new RequestGeo(
            context.Request.Headers["X-Geo-Country"].FirstOrDefault() ?? "Unknown",
            context.Request.Headers["X-Geo-Region"].FirstOrDefault() ?? "Unknown",
            context.Request.Headers["X-Geo-City"].FirstOrDefault() ?? "Unknown",
            ipAddress,
            ParseNullableDouble(context.Request.Headers["X-Geo-Latitude"].FirstOrDefault()),
            ParseNullableDouble(context.Request.Headers["X-Geo-Longitude"].FirstOrDefault()));
    }

    private string ResolveDatabasePath()
    {
        if (Path.IsPathRooted(_settings.DatabasePath))
        {
            return _settings.DatabasePath;
        }

        return Path.Combine(_environment.ContentRootPath, _settings.DatabasePath);
    }

    private static string ResolveClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }

    private static RequestGeo Unknown(string ipAddress)
    {
        return new RequestGeo("Unknown", "Unknown", "Unknown", ipAddress, null, null);
    }

    private static double? ParseNullableDouble(string? value)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
