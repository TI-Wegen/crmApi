using CRM.Infrastructure.Database;
using Hangfire;
using Hangfire.PostgreSql;
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
        services.AddHangFire(config);
        return services;
    }

    private static IServiceCollection AddDbConnection(
         this IServiceCollection services,
         IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
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

    public static IServiceCollection AddHangFire(
     this IServiceCollection services,
         IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

        return services;
    }

}