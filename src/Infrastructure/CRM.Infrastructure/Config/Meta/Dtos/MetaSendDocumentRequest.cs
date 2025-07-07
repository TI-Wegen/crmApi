using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos;

public record MetaSendDocumentRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; } = "whatsapp";

    // NOVO: Adicionado para conformidade com a documentação
    [JsonPropertyName("recipient_type")]
    public string RecipientType { get; } = "individual";

    [JsonPropertyName("to")]
    public string To { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; } = "document";

    [JsonPropertyName("document")]
    public DocumentPayload Document { get; init; }

    public MetaSendDocumentRequest(string to, string link, string filename, string? caption)
    {
        To = to;
        Document = new DocumentPayload(link, filename, caption);
    }
}

// Objeto aninhado para os detalhes do documento
public record DocumentPayload
{
    [JsonPropertyName("link")]
    public string Link { get; init; }

    [JsonPropertyName("filename")]
    public string Filename { get; init; }

    // O '?' torna a legenda opcional. Se for nula, não será serializada no JSON.
    [JsonPropertyName("caption")]
    public string? Caption { get; init; }

    public DocumentPayload(string link, string filename, string? caption)
    {
        Link = link;
        Filename = filename;
        Caption = caption;
    }
}