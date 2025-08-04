using Conversations.Application.Abstractions;
using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Config.Meta.Dtos;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        var requestBody = new MetaSendMessageRequest(numeroDestino, textoMensagem);


        var jsonContent = new StringContent(
          JsonSerializer.Serialize(requestBody),
          Encoding.UTF8,
          "application/json"
      );

        var response = await httpClient.PostAsync(requestUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"--> Erro ao enviar mensagem pela API da Meta: {errorContent}");
            throw new Exception("Falha ao enviar mensagem pela API da Meta.");
        }

        Console.WriteLine("--> Mensagem enviada com sucesso pela API da Meta!");
    }

    public async Task EnviarDocumentoAsync(string numeroDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        var requestBody = new MetaSendDocumentRequest(numeroDestino, urlDoDocumento, nomeDoArquivo, legenda);

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(requestUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"--> Erro ao enviar DOCUMENTO pela API da Meta: {responseContent}");
            throw new Exception("Falha ao enviar documento pela API da Meta.");
        }

        Console.WriteLine($"--> Resposta de SUCESSO da Meta: {responseContent}");
    }
    public async Task EnviarImagemAsync(string numeroDestino, string urlDaImagem, string? legenda)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        var requestBody = new MetaSendImageRequest(numeroDestino, urlDaImagem, legenda);

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(requestUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"--> Erro ao enviar IMAGEM pela API da Meta: {errorContent}");
            throw new Exception("Falha ao enviar imagem pela API da Meta.");
        }

        Console.WriteLine("--> Imagem enviada com sucesso pela API da Meta!");
    }

    public async Task EnviarAudioAsync(string numeroDestino, string urlDoAudio)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        var requestBody = new MetaSendAudioRequest(numeroDestino, urlDoAudio);

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(requestUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"--> Erro ao enviar ÁUDIO pela API da Meta: {errorContent}");
            throw new Exception("Falha ao enviar áudio pela API da Meta.");
        }

        Console.WriteLine("--> Áudio enviado com sucesso pela API da Meta!");
    }
    public async Task<string> EnviarTemplateAsync(string numeroDestino, string templateName, List<string> bodyParameters)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        var requestBody = new MetaSendTemplateRequest(numeroDestino, templateName, bodyParameters);

        var serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody, serializerOptions), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"--> Erro ao enviar TEMPLATE pela API da Meta: {responseContent}");
            throw new Exception("Falha ao enviar template pela API da Meta.");
        }

        Console.WriteLine($"--> Resposta de SUCESSO da Meta (Template): {responseContent}");

        using var jsonDoc = JsonDocument.Parse(responseContent);
        var messageId = jsonDoc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString();

        return messageId ?? string.Empty;
    }

    
        public async Task EnviarPesquisaDeSatisfacaoAsync(string numeroDestino, Guid atendimentoId)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessPhoneNumberId}/messages";

        var action = new ActionPayload(new List<ButtonPayload>
    {
        new(new ReplyPayload($"rating_{atendimentoId}_5", "⭐ Ótimo")),
        new(new ReplyPayload($"rating_{atendimentoId}_3", "😐 Razoável")),
        new(new ReplyPayload($"rating_{atendimentoId}_1", "👎 Ruim"))
    });


        var body = new InteractiveBodyPayload("Ficamos felizes em ajudar! Como você avalia nosso atendimento?");
        var interactive = new InteractivePayload(body, action);

        var requestBody = new MetaSendInteractiveRequest(numeroDestino, interactive);

        var serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody, serializerOptions), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"--> Erro ao enviar PESQUISA DE SATISFAÇÃO pela API da Meta: {responseContent}");
            throw new Exception("Falha ao enviar pesquisa de satisfação pela API da Meta.");
        }
    }
}