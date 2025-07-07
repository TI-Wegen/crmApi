namespace CRM.Infrastructure.Config.Meta.Dtos;

using System.Text.Json.Serialization;

public record MetaSendImageRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; } = "whatsapp";

    [JsonPropertyName("recipient_type")]
    public string RecipientType { get; } = "individual";

    [JsonPropertyName("to")]
    public string To { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; } = "image";

    [JsonPropertyName("image")]
    public ImagePayload Image { get; init; }

    public MetaSendImageRequest(string to, string link, string? caption)
    {
        To = to;
        Image = new ImagePayload(link, caption);
    }
}

// Objeto aninhado para os detalhes da imagem
public record ImagePayload
{
    [JsonPropertyName("link")]
    public string Link { get; init; }

    [JsonPropertyName("caption")]
    public string? Caption { get; init; }

    public ImagePayload(string link, string? caption)
    {
        Link = link;
        Caption = caption;
    }
}