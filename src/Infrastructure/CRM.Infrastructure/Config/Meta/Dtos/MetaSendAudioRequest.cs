using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos;

public record MetaSendAudioRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; } = "whatsapp";

    [JsonPropertyName("recipient_type")] public string RecipientType { get; } = "individual";

    [JsonPropertyName("to")] public string To { get; init; }

    [JsonPropertyName("type")] public string Type { get; } = "audio";

    [JsonPropertyName("audio")] public AudioPayload Audio { get; init; }

    public MetaSendAudioRequest(string to, string link)
    {
        To = to;
        Audio = new AudioPayload(link);
    }
}

public record AudioPayload
{
    [JsonPropertyName("link")] public string Link { get; init; }

    public AudioPayload(string link)
    {
        Link = link;
    }
}