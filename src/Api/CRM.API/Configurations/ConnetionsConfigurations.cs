using Conversations.Application.Abstractions;
using CRM.API.Services;
using CRM.Infrastructure.Database;
using CRM.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace CRM.API.Configurations;

public static class ConnectionsConfigurations
{
    public static IServiceCollection AddAppConnections(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbConnection(config);
        //services.AddInMemoryConnections();
        services.AddHangFire(config);
        services.AddFileStorage(config);
        services.AddSignalRConnection(config);
        services.AddDapperConnection(config);
        return services;
    }

    private static IServiceCollection AddDbConnection(
         this IServiceCollection services,
         IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(
            options => options.UseNpgsql(connectionString)
             .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

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

    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
     IConfiguration config)
    {
        services.Configure<MinioSettings>(config.GetSection("MinioSettings"));
        services.AddSingleton<IFileStorageService, MinioStorageService>();
        return services;
    }

    public static IServiceCollection AddSignalRConnection(
    this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
        return services;
    }
    public static IServiceCollection AddDapperConnection (
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));
        return services;
    }
}