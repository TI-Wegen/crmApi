using CRM.API.Configurations;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddAppConnections(builder.Configuration)
    .AddUseCases();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI( options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CRM API");
    });
}


app.UseHttpsRedirection();
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();

app.MapControllers();



app.Run();
