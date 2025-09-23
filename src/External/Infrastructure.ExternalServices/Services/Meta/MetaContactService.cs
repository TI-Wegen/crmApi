using Contacts.Application.Abstractions;
using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Config.Meta.Dtos;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Services.Meta;

public record MetaContactProfileDto(
    [property: JsonPropertyName("profile_picture_url")]
    string ProfilePictureUrl);

public record ContactCheckResult(
    [property: JsonPropertyName("input")] string Input,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("wa_id")] string WaId);

public class MetaContactService : IMetaContactService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetaSettings _metaSettings;

    public MetaContactService(IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaSettings)
    {
        _httpClientFactory = httpClientFactory;
        _metaSettings = metaSettings.Value;
    }

    public async Task<string?> GetProfilePictureUrlAsync(string waId)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{waId}?fields=profile_picture_url";

        var response = await httpClient.GetAsync(requestUrl);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) return null;

        var profileInfo = JsonSerializer.Deserialize<MetaContactProfileDto>(content);

        return profileInfo?.ProfilePictureUrl;
    }

    public async Task<string?> VerifyContactAndGetWaIdAsync(string phoneNumber)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{_metaSettings.WhatsAppBusinessAccountId}/contacts";

        var requestBody = new
        {
            blocking = "wait",
            contacts = new[] { phoneNumber }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(requestUrl, jsonContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao verificar contato: {content}");
        }

        var result = JsonSerializer.Deserialize<MetaContactCheckResponse>(content);
        return result?.Contacts?.FirstOrDefault(c => c.Status == "valid")?.WaId;
    }
}