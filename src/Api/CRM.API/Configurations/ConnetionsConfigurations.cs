using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM.API.Configurations;

public static class ConnectionsConfigurations
{
    public static IServiceCollection AddAppConnections(
        this IServiceCollection services,
        IConfiguration config)
    {
        //services.AddDbConnection(config);
        services.AddInMemoryConnections();

        return services;
    }

    private static IServiceCollection AddDbConnection(
         this IServiceCollection services,
         IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Postgress");
        services.AddDbContext<AppDbContext>(
            options => options.UseNpgsql(connectionString));

        return services;
    }
    public static IServiceCollection AddInMemoryConnections(
        this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(
            options => options
                .EnableSensitiveDataLogging()
                .UseInMemoryDatabase("Db-In-Memory-dev"));

        return services;
    }


}