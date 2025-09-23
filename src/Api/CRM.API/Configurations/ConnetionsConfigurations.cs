using Agents.Infrastructure.Services;
using Amazon;
using Amazon.S3;
using Conversations.Infrastructure.Services;
using CRM.API.Services;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Database;
using CRM.Infrastructure.Storage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using System.Data;
using System.Text;
using Conversations.Application.Repositories;

namespace CRM.API.Configurations;

public static class ConnectionsConfigurations
{
    public static IServiceCollection AddAppConnections(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbConnection(config);
        services.AddHangFire(config);
        services.AddFileStorage(config);
        services.AddSignalRConnection(config);
        services.AddDapperConnection(config);
        services.AddMetaConnection(config);
        services.AddJwtBearer(config);
        services.AddRedisConnection(config);
        services.AddHealtChecks(config);
        return services;
    }

    private static IServiceCollection AddDbConnection(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,                
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null             
                    );
                })
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
        );


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

        services.AddHangfireServer();


        return services;
    }

    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<S3Settings>(config.GetSection("S3Config"));
        services.AddSingleton<IFileStorageService, S3FileStorageService>();
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

    public static IServiceCollection AddDapperConnection(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));
        return services;
    }

    private static IServiceCollection AddRedisConnection(
        this IServiceCollection services,
        IConfiguration config)
    {
        var redisConnectionString = config["RedisConnectionString"];
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("A connection string 'RedisConnectionString' não foi encontrada.");
        }
        
        var uri = new Uri(redisConnectionString);

        var redisConfig = new ConfigurationOptions
        {
            EndPoints = { { uri.Host, uri.Port } },
            Ssl = uri.Scheme == "rediss",
            User = uri.UserInfo.Split(':')[0],
            Password = uri.UserInfo.Split(':')[1],
            AbortOnConnectFail = false,
            ConnectTimeout = 10000,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                           System.Security.Authentication.SslProtocols.Tls13
        };
        
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));

        return services;
    }

    private static IServiceCollection AddHealtChecks(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString: config.GetConnectionString("DefaultConnection"),
                name: "PostgreSQL Principal",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "critical" })
                
            .AddMySql(
                connectionString: config.GetConnectionString("ExternalConnection"),
                name: "MySQL Externo (Boletos)",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "database", "external" })
                
            .AddRedis(
                sp => sp.GetRequiredService<IConnectionMultiplexer>(),
                name: "Redis Cache",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "cache", "critical" })
                
            .AddS3(options =>
                {
                    var s3Config = config.GetSection("S3Config");
                    
                    options.AccessKey = s3Config["AccessKey"];
                    options.SecretKey = s3Config["SecretKey"];
                    options.BucketName = s3Config["BucketName"];

                    options.S3Config = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.GetBySystemName(s3Config["Region"])
                    };
                },
                name: "AWS S3 Storage",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "storage", "critical" });


        services
            .AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(60);
                options.MaximumHistoryEntriesPerEndpoint(50);
                options.AddHealthCheckEndpoint("API Wegen CRM", "/health");
            })
            .AddInMemoryStorage();


        return services;
    }

    public static IServiceCollection AddMetaConnection(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddHttpClient("MetaApiClient", client =>
        {
            var metaSettings = config.GetSection("MetaSettings").Get<MetaSettings>();
            
            client.BaseAddress = new Uri(metaSettings.BaseUrl);
            
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", metaSettings.AccessToken);
        });
        services.AddScoped<IMetaMessageSender, MetaMessageSender>();
        return services;
    }

    private static IServiceCollection AddJwtBearer(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                        .GetBytes(config["JwtSettings:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JwtSettings:Audience"]
                };
                options.MapInboundClaims = false;
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/conversationHub")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}