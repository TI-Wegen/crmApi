using Agents.Infrastructure.Services;
using Conversations.Application.Abstractions;
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
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using System.Data;
using System.Text;

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
    public static IServiceCollection AddDapperConnection (
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));
        return services;
    }
    public static IServiceCollection AddRedisConnection(
    this IServiceCollection services,
    IConfiguration config)
    {
       services.AddSingleton<IConnectionMultiplexer>(
         ConnectionMultiplexer.Connect(config["RedisConnectionString"]));

        // Registra nosso serviço de cache
       services.AddScoped<IBotSessionCache, RedisBotSessionCache>();
        return services;
    }

    public static IServiceCollection AddMetaConnection(
    this IServiceCollection services,
    IConfiguration config)
    {
        services.AddHttpClient("MetaApiClient", client =>
        {
            // Lê as configurações do appsettings.json
            var metaSettings = config.GetSection("MetaSettings").Get<MetaSettings>();

            // 1. Define a URL base para todas as chamadas feitas com este cliente
            client.BaseAddress = new Uri(metaSettings.BaseUrl);

            // 2. AQUI ESTÁ A INJEÇÃO DO TOKEN!
            // Define o cabeçalho de autorização PADRÃO para todas as chamadas
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

                // ---- BLOCO ADICIONADO PARA O SIGNALR ----
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Tenta ler o token da query string
                        var accessToken = context.Request.Query["access_token"];

                        // Se a requisição for para um hub SignalR
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/conversationHub"))) // Adicione outros hubs se necessário
                        {
                            // Atribui o token ao contexto para validação
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
                // ---- FIM DO BLOCO ADICIONADO ----
            });

        services.AddAuthorization();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }

}