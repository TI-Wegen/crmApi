using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CRM.API.Middlewares;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseWrapperMiddleware> _logger;
    
    private readonly HashSet<int> _excludedStatusCodes = new()
    {
        204,
        304 
    };
    
    private static readonly Dictionary<int, string> StatusCodeMessages = new()
    {
        { 200, "Operação realizada com sucesso" },
        { 201, "Recurso criado com sucesso" },
        { 202, "Solicitação aceita para processamento" },
        { 204, "Operação realizada sem conteúdo de retorno" },
            
        { 400, "Solicitação inválida" },
        { 401, "Não autorizado" },
        { 403, "Acesso proibido" },
        { 404, "Recurso não encontrado" },
        { 405, "Método não permitido" },
        { 409, "Conflito na operação" },
        { 410, "Recurso não está mais disponível" },
        { 422, "Erro de validação" },
        { 429, "Muitas solicitações" },
            
        { 500, "Erro interno do servidor" },
        { 501, "Funcionalidade não implementada" },
        { 502, "Gateway inválido" },
        { 503, "Serviço indisponível" },
        { 504, "Timeout do gateway" }
    };

    public ResponseWrapperMiddleware(RequestDelegate next, ILogger<ResponseWrapperMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipWrapping(context))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            
            if (_excludedStatusCodes.Contains(statusCode))
            {
                await CopyResponseToOriginalStream(responseBody, originalBodyStream);
                return;
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(responseBody).ReadToEndAsync();

            var responseWrapper = CreateResponseWrapper(statusCode, bodyText);
            await WriteResponseAsync(context, responseWrapper, statusCode, originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado durante o processamento da requisição");
            context.Response.Body = originalBodyStream;
            await HandleExceptionAsync(context, ex);
        }
    }

    private bool ShouldSkipWrapping(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        if (path.Contains("/hub") || path.Contains("/conversationhub"))
            return true;

        if (context.WebSockets.IsWebSocketRequest)
            return true;

        return path.StartsWith("/swagger") ||
               path.StartsWith("/health") ||
               path.StartsWith("/metrics") ||
               context.Request.Headers.ContainsKey("X-Skip-Response-Wrapper") ||
               context.Response.ContentType?.StartsWith("text/html") == true ||
               context.Response.ContentType?.StartsWith("application/octet-stream") == true;
    }


    private object CreateResponseWrapper(int statusCode, string bodyText)
    {
        object? data = null;
        
        if (!string.IsNullOrWhiteSpace(bodyText))
        {
            data = TryParseJson(bodyText);
        }

        return statusCode switch
        {
            >= 200 and < 300 => CreateSuccessResponse(statusCode, data),
            400 or 422 => CreateValidationErrorResponse(statusCode, data),
            401 => CreateErrorResponse(statusCode, "Credenciais inválidas ou token expirado", data),
            403 => CreateErrorResponse(statusCode, "Você não tem permissão para acessar este recurso", data),
            404 => CreateErrorResponse(statusCode, "O recurso solicitado não foi encontrado", data),
            409 => CreateErrorResponse(statusCode, "Conflito: o recurso não pode ser processado no estado atual", data),
            429 => CreateErrorResponse(statusCode, "Limite de requisições excedido. Tente novamente em alguns minutos", data),
            >= 400 and < 500 => CreateClientErrorResponse(statusCode, data),
            >= 500 => CreateServerErrorResponse(statusCode, data),
            _ => CreateGenericResponse(statusCode, data)
        };
    }

    private object CreateSuccessResponse(int statusCode, object? data)
    {
        var message = StatusCodeMessages.GetValueOrDefault(statusCode, "Operação realizada com sucesso");
        
        return new
        {
            success = true,
            message,
            data,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
    }

    private object CreateValidationErrorResponse(int statusCode, object? data)
    {
        Dictionary<string, string[]>? validationErrors = null;
        string message = "Erro de validação";

        if (data is JsonElement jsonElement && jsonElement.TryGetProperty("errors", out var errorsElement))
        {
            validationErrors = ExtractValidationErrors(errorsElement);
        }
        else if (data is JsonElement singleError && singleError.TryGetProperty("title", out var titleElement))
        {
            message = titleElement.GetString() ?? message;
        }

        return new
        {
            success = false,
            message,
            errors = validationErrors ?? ExtractErrorsFromData(data),
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
    }

    private object CreateClientErrorResponse(int statusCode, object? data)
    {
        var message = StatusCodeMessages.GetValueOrDefault(statusCode, "Erro na solicitação");
        var errors = ExtractErrorsFromData(data);

        return new
        {
            success = false,
            message,
            errors,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
    }

    private object CreateServerErrorResponse(int statusCode, object? data)
    {
        var message = StatusCodeMessages.GetValueOrDefault(statusCode, "Erro interno do servidor");
        
        var errors = IsProduction() 
            ? new { detail = "Ocorreu um erro interno. Entre em contato com o suporte." }
            : ExtractErrorsFromData(data);

        return new
        {
            success = false,
            message,
            errors,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
    }

    private object CreateErrorResponse(int statusCode, string message, object? data)
    {
        var errors = ExtractErrorsFromData(data);

        return new
        {
            success = false,
            message,
            errors,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
    }

    private object CreateGenericResponse(int statusCode, object? data)
    {
        var isSuccess = statusCode < 400;
        var message = StatusCodeMessages.GetValueOrDefault(statusCode, 
            isSuccess ? "Operação processada" : "Erro no processamento");

        var response = new
        {
            success = isSuccess,
            message,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };
        
        return isSuccess 
            ? (object)new { response.success, response.message, data, response.statusCode, response.timestamp }
            : new { response.success, response.message, errors = ExtractErrorsFromData(data), response.statusCode, response.timestamp };
    }

    private object? TryParseJson(string bodyText)
    {
        try
        {
            return JsonSerializer.Deserialize<object>(bodyText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return bodyText;
        }
    }

    private object ExtractErrorsFromData(object? data)
    {
        if (data == null) return new { };

        if (data is JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty("detail", out var detailElement))
                return new { detail = detailElement.GetString() };
            
            if (jsonElement.TryGetProperty("message", out var messageElement))
                return new { detail = messageElement.GetString() };

            return new { detail = jsonElement.ToString() };
        }

        return new { detail = data.ToString() };
    }

    private Dictionary<string, string[]> ExtractValidationErrors(JsonElement errorsElement)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errorsElement.EnumerateObject())
            {
                var errorMessages = property.Value.ValueKind switch
                {
                    JsonValueKind.Array => property.Value.EnumerateArray()
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Cast<string>()
                        .ToArray(),
                    
                    JsonValueKind.String => new[] { property.Value.GetString()! },
                    
                    _ => new[] { property.Value.ToString() }
                };

                if (errorMessages.Length > 0)
                {
                    errors[property.Name] = errorMessages;
                }
            }
        }

        return errors;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => 401,
            ArgumentException or ArgumentNullException => 400,
            NotImplementedException => 501,
            TimeoutException => 504,
            _ => 500
        };

        object errors;
        if (IsProduction())
        {
            errors = new { detail = "Ocorreu um erro interno. Entre em contato com o suporte." };
        }
        else
        {
            errors = new { detail = exception.Message, stackTrace = exception.StackTrace };
        }

        var response = new
        {
            success = false,
            message = StatusCodeMessages.GetValueOrDefault(statusCode, "Erro interno do servidor"),
            errors,
            statusCode,
            timestamp = DateTimeOffset.UtcNow
        };

        await WriteResponseAsync(context, response, statusCode, context.Response.Body);
    }

    private async Task WriteResponseAsync(HttpContext context, object response, int statusCode, Stream outputStream)
    {
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        context.Response.Body = outputStream;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var bytes = Encoding.UTF8.GetBytes(jsonResponse);
        context.Response.ContentLength = bytes.Length;
        
        await context.Response.Body.WriteAsync(bytes);
    }

    private async Task CopyResponseToOriginalStream(MemoryStream responseBody, Stream originalStream)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalStream);
    }

    private bool IsProduction()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant() == "production";
    }
}