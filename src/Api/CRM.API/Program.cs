using Conversations.Infrastructure.Jobs;
using CRM.API.Configurations;
using CRM.API.Hubs;
using CRM.Infrastructure.Config.Meta;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

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

app.UseGlobalExceptionMiddleware();

app.UseCors("DefaultCorsPolicy");

app.UseWebSockets();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM.API v1");
        c.RoutePrefix = string.Empty; // Abre direto na raiz (http://localhost:5000/)
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