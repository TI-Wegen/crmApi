using CRM.API.FilterException;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace CRM.API.Configurations;

public static class ApiServiceExtensions
{
    // Método principal que orquestra as configurações
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
            options.Filters.Add(typeof(ApiGlobalExceptionFilter)));

        services.AddSwaggerDocumentation(); // Responsabilidade 1
        services.AddJwtAuthentication(configuration); // Responsabilidade 2

        services.AddEndpointsApiExplorer();
        return services;
    }

    // Método focado apenas na documentação
    private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CRM.API", Version = "v1.0.0" });
            //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Name = "Authorization",
            //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.ApiKey,
            //    Scheme = "Bearer",
            //    BearerFormat = "JWT"
            //});

            //c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = "Bearer"
            //            }
            //        },
            //        new string[] {}
            //    }
            //});
        });
        return services;
    }

    // Método focado apenas na autenticação
    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var secret = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                };
            });
        return services;
    }

    // Método para configurar o pipeline, com a correção já aplicada
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM.API v1.0.0");
            });
        }
        return app;
    }
}