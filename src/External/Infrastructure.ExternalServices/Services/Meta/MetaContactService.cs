using Contacts.Application.Abstractions;
using CRM.Infrastructure.Config.Meta;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ExternalServices.Services.Meta;

public record MetaContactProfileDto([property: JsonPropertyName("profile_picture_url")] string ProfilePictureUrl);

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
        // A query agora pede o campo específico 'profile_picture_url'
        var requestUrl = $"{_metaSettings.MetaApiVersion}/{waId}?fields=profile_picture_url";

        var response = await httpClient.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var profileInfo = JsonSerializer.Deserialize<MetaContactProfileDto>(content);

        return profileInfo?.ProfilePictureUrl;
    }
}
