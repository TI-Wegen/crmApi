namespace Templates.Infrastructure.Services;

using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Config.Meta.Dtos;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Templates.Application.Abstractions;
using Templates.Domain.Aggregates;

public class MetaTemplateManager : IMetaTemplateManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetaSettings _metaSettings;

    public MetaTemplateManager(IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaSettings)
    {
        _httpClientFactory = httpClientFactory;
        _metaSettings = metaSettings.Value;
    }

    public async Task CriarTemplateNaMetaAsync(MessageTemplate template)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessAccountId}/message_templates";

        var requestBody = new MetaCreateTemplateRequest
        {
            Name = template.Name.ToLower(), 
            Language = template.Language,
            Category = "UTILITY", 
            Components = new List<TemplateComponent>
            {
                new() { Type = "BODY", Text = template.Body }
                // Adicionar lógica para HEADER (imagem/documento) ou BUTTONS aqui se necessário
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUrl, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"--> Erro ao criar TEMPLATE na API da Meta: {responseContent}");
            throw new Exception($"Falha ao criar template na API da Meta: {responseContent}");
        }

        Console.WriteLine("--> Template submetido para aprovação com sucesso!");
    }
}