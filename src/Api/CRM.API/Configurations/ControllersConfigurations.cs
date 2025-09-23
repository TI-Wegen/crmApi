using System.Text;
using CRM.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CRM.API.Configurations;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddSwaggerDocumentation();
        services.AddJwtAuthentication(configuration);

        services.AddEndpointsApiExplorer();
        return services;
    }

    private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "CRM.API", 
                Version = "v1",
                Description = "API do Sistema CRM"
            });
        });
        return services;
    }



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

    public static WebApplication UseGlobalExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ResponseWrapperMiddleware>();

        return app;
    }
}