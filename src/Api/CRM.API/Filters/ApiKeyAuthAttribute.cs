namespace CRM.API.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
private const string ApiKeyHeaderName = "X-Api-Key";

public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
    {
        context.Result = new UnauthorizedResult();
        return;
    }

    var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
    var apiKey = configuration.GetValue<string>("InternalApiSettings:ApiKey");

    if (!apiKey.Equals(potentialApiKey))
    {
        context.Result = new UnauthorizedResult();
        return;
    }

    await next();
}
}
