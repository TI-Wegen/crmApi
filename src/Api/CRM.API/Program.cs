using CRM.API.Configurations;
using CRM.API.Hubs;
using CRM.Infrastructure.Config.Meta;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var frontEndUrl = "http://localhost:3000";

builder.Services.Configure<MetaSettings>(builder.Configuration.GetSection("MetaSettings"));

builder.Services
    .AddAppConnections(builder.Configuration)
    .AddUseCases();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextAppPolicy", policy =>
    {
        policy.WithOrigins(frontEndUrl)  // IMPORTANTE: Substitui .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();      // ESSENCIAL: Adicione esta linha
    });
});
var app = builder.Build();

app.UseCors("AllowNextAppPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI( options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CRM API");
    });
}
//app.UseHealthChecks("/health");

app.UseHangfireDashboard("/hangfire");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ConversationHub>("/conversationHub"); 


app.Run();
