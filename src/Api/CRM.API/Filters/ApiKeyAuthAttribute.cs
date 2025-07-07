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
    // Pega a chave da requisição que chegou
    if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
    {
        context.Result = new UnauthorizedResult();
        return;
    }

    // Pega a chave que esperamos da nossa configuração
    var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
    var apiKey = configuration.GetValue<string>("InternalApiSettings:ApiKey");

    // Compara as chaves
    if (!apiKey.Equals(potentialApiKey))
    {
        context.Result = new UnauthorizedResult();
        return;
    }

    // Se as chaves baterem, permite que a requisição continue
    await next();
}
}
