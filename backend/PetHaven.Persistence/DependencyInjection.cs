using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetHaven.Application.Interfaces;
using PetHaven.Persistence.Context;
using PetHaven.Persistence.UnitOfWork;

namespace PetHaven.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>),
            typeof(Repositories.Repository<>));

        services.AddScoped<IUnitOfWork,
            UnitOfWork.UnitOfWork>();

        return services;
    }
}