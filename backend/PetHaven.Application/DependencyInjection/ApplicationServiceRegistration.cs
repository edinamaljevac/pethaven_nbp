using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using MediatR;
using PetHaven.Application.Behaviors;

namespace PetHaven.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        var mapperConfiguration = new MapperConfiguration(
            cfg => cfg.AddMaps(assembly),
            NullLoggerFactory.Instance);

        services.AddSingleton(mapperConfiguration);
        services.AddSingleton<IMapper>(sp =>
            mapperConfiguration.CreateMapper());

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}