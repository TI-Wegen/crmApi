using Conversations.Application.Abstractions;
using CRM.Infrastructure.Config.Meta;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conversations.Infrastructure.Services;

    public class MetaMediaService: IMetaMediaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MetaSettings _metaSettings;

    public MetaMediaService(IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaSettings)
    {
        _httpClientFactory = httpClientFactory;
        _metaSettings = metaSettings.Value;
    }

    public async Task<MediaFile?> DownloadMediaAsync(string mediaId)
    {
        var httpClient = _httpClientFactory.CreateClient("MetaApiClient");

        // ETAPA 1: Obter a URL da mídia
        var mediaInfoUrl = $"{_metaSettings.MetaApiVersion}/{mediaId}";
        var mediaInfoResponse = await httpClient.GetAsync(mediaInfoUrl);
        if (!mediaInfoResponse.IsSuccessStatusCode)
        {
            // Logar o erro aqui
            return null;
        }

        var mediaInfoContent = await mediaInfoResponse.Content.ReadAsStringAsync();
        var mediaInfo = JsonSerializer.Deserialize<MetaMediaInfoResponse>(mediaInfoContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (mediaInfo is null || string.IsNullOrEmpty(mediaInfo.Url))
        {
            return null;
        }

        // ETAPA 2: Baixar o arquivo da URL obtida
        // Note que o mesmo cliente com o token de autorização é usado
        var fileResponse = await httpClient.GetAsync(mediaInfo.Url);
        if (!fileResponse.IsSuccessStatusCode)
        {
            return null;
        }
        var mimeTypeFinal = fileResponse.Content.Headers.ContentType?.MediaType ?? mediaInfo.MimeType;

        var fileStream = await fileResponse.Content.ReadAsStreamAsync();
        var fileName = $"media_{mediaId}.{GetFileExtension(mimeTypeFinal)}";

        return new MediaFile(fileStream, mimeTypeFinal, fileName);
    }

    private string GetFileExtension(string? mimeType)
    {
        // Adiciona uma verificação para nulo ou vazio antes de tentar o Split.
        if (string.IsNullOrEmpty(mimeType))
        {
            return "tmp"; // Retorna uma extensão padrão se o mimeType for inválido.
        }
        return mimeType.Split('/').LastOrDefault() ?? "tmp";
    }
}

public record MetaMediaInfoResponse
(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("mime_type")] string MimeType
);


