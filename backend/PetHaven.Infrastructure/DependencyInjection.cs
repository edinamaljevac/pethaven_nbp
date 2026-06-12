using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetHaven.Application.Interfaces;
using PetHaven.Application.Settings;
using PetHaven.Infrastructure.Services;

namespace PetHaven.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));
        services.Configure<CloudinarySettings>(
            configuration.GetSection("Cloudinary"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        // Use Cloudinary when credentials are configured (required on hosts with an
        // ephemeral filesystem); otherwise fall back to local disk for development.
        var cloudinarySettings = configuration.GetSection("Cloudinary").Get<CloudinarySettings>();
        if (cloudinarySettings?.IsConfigured == true)
        {
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }
        services.AddScoped<IAdoptionContractGenerator, AdoptionContractGenerator>();
        services.AddSingleton(_ =>
        {
            var client = new HttpClient { BaseAddress = new Uri("https://nominatim.openstreetmap.org/") };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("PetHaven/1.0");
            return client;
        });
        services.AddScoped<IGeocodingService, NominatimGeocodingService>();

        return services;
    }
}
