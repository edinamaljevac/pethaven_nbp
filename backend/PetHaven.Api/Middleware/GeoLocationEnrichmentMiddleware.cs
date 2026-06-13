using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.Extensions.Options;
using PetHaven.Api.Settings;
using System.Net;
using System.Net.Sockets;

namespace PetHaven.Api.Middleware;

public class GeoLocationEnrichmentMiddleware
{
    public const string ContextKey = "RequestGeo";
    private readonly GeoLocationSettings _settings;
    private readonly ILogger<GeoLocationEnrichmentMiddleware> _logger;
    private readonly RequestDelegate _next;

    private readonly DatabaseReader? _reader;

    public GeoLocationEnrichmentMiddleware(
        RequestDelegate next,
        IOptions<GeoLocationSettings> settings,
        IWebHostEnvironment environment,
        ILogger<GeoLocationEnrichmentMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;

        var databasePath = ResolveDatabasePath(environment);
        if (File.Exists(databasePath))
        {
            try
            {
                _reader = new DatabaseReader(databasePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open MaxMind GeoLite2 database at {DatabasePath}. Falling back to header/default location.", databasePath);
            }
        }
        else
        {
            _logger.LogWarning("MaxMind GeoLite2 database not found at {DatabasePath}. Geo enrichment will use the configured fallback location.", databasePath);
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = ResolveClientIp(context);
        context.Items[ContextKey] = ResolveGeo(context, ipAddress);
        await _next(context);
    }

    private RequestGeo ResolveGeo(HttpContext context, string ipAddress)
    {
        var headerGeo = ResolveFromHeaders(context, ipAddress);
        if (headerGeo is not null)
        {
            return headerGeo;
        }

        if (_reader is not null
            && IPAddress.TryParse(ipAddress, out var parsedIp)
            && !IsPrivateOrLoopback(parsedIp))
        {
            try
            {
                var city = _reader.City(parsedIp);
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not resolve geolocation for IP {IpAddress} using MaxMind database.", ipAddress);
            }
        }

        return FallbackGeo(ipAddress);
    }

    private RequestGeo? ResolveFromHeaders(HttpContext context, string ipAddress)
    {
        var country = context.Request.Headers["X-Geo-Country"].FirstOrDefault();
        var region = context.Request.Headers["X-Geo-Region"].FirstOrDefault();
        var city = context.Request.Headers["X-Geo-City"].FirstOrDefault();
        var latitude = ParseNullableDouble(context.Request.Headers["X-Geo-Latitude"].FirstOrDefault());
        var longitude = ParseNullableDouble(context.Request.Headers["X-Geo-Longitude"].FirstOrDefault());

        if (string.IsNullOrWhiteSpace(country)
            && string.IsNullOrWhiteSpace(region)
            && string.IsNullOrWhiteSpace(city)
            && latitude is null
            && longitude is null)
        {
            return null;
        }

        return new RequestGeo(
            country ?? "Unknown",
            region ?? "Unknown",
            city ?? "Unknown",
            ipAddress,
            latitude,
            longitude);
    }

    private RequestGeo FallbackGeo(string ipAddress)
    {
        var fallback = _settings.FallbackLocation;
        if (fallback is null)
        {
            return new RequestGeo("Unknown", "Unknown", "Unknown", ipAddress, null, null);
        }

        return new RequestGeo(
            fallback.Country,
            fallback.Region,
            fallback.City,
            ipAddress,
            fallback.Latitude,
            fallback.Longitude);
    }

    private string ResolveDatabasePath(IWebHostEnvironment environment)
    {
        return Path.IsPathRooted(_settings.DatabasePath)
            ? _settings.DatabasePath
            : Path.Combine(environment.ContentRootPath, _settings.DatabasePath);
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

    private static bool IsPrivateOrLoopback(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || (bytes[0] == 169 && bytes[1] == 254);
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || (ip.GetAddressBytes()[0] & 0xFE) == 0xFC;
        }

        return false;
    }

    private static double? ParseNullableDouble(string? value)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}