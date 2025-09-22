using Conversations.Infrastructure.Jobs;
using CRM.API.Configurations;
using CRM.API.Hubs;
using CRM.Domain.Exceptions;
using CRM.Infrastructure.Config.Meta;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins == null || !allowedOrigins.Any())
{
    throw new InvalidOperationException("Configuração de 'AllowedOrigins' para o CORS não encontrada ou vazia.");
}

builder.Services.Configure<MetaSettings>(builder.Configuration.GetSection("MetaSettings"));

builder.Services
    .AddAppConnections(builder.Configuration)
    .AddUseCases();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>();

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("DefaultCorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CRM API");
    });
}

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecksUI(options =>
{
    options.UIPath = "/monitor";
    options.ApiPath = "/monitor-api";
});

app.UseHangfireDashboard("/hangfire");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ConversationHub>("/conversationHub");

app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    RecurringJob.AddOrUpdate<CleanExpiredBotSessionsJob>(
        recurringJobId: "clean-expired-bot-sessions",
        methodCall: job => job.Executar(),
        cronExpression: "*/5 * * * *");
});

app.Run();
