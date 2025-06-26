using Conversations.Application.Abstractions;
using CRM.Infrastructure.Config;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Conversations.Infrastructure.Services;

public class MetaMessageSender : IMetaMessageSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetaSettings _metaSettings;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MetaMessageSender(IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaSettings)
    {
        _httpClientFactory = httpClientFactory;
        _metaSettings = metaSettings.Value;
    }

    public async Task EnviarMensagemTextoAsync(string numeroDestino, string textoMensagem)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");

        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        // Monta o corpo da requisição que a API da Meta espera
        var requestBody = new
        {
            MessagingProduct = "whatsapp",
            To = numeroDestino,
            Type = "text",
            Text = new { Body = textoMensagem }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(requestUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"--> Erro ao enviar mensagem pela API da Meta: {errorContent}");
            // Em produção, você lançaria uma exceção customizada aqui.
            throw new Exception("Falha ao enviar mensagem pela API da Meta.");
        }

        Console.WriteLine("--> Mensagem enviada com sucesso pela API da Meta!");
    }
}