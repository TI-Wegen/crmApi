namespace CRM.Infrastructure.Config.Meta.Dtos;



using System.Text.Json.Serialization;


public record MetaCreateTemplateRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } // Ex: "UTILITY", "MARKETING"

    [JsonPropertyName("components")]
    public List<TemplateComponent> Components { get; set; }
}

public record TemplateComponent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } // "BODY", "HEADER", etc.

    [JsonPropertyName("text")]
    public string? Text { get; set; } // Para componentes de texto

    [JsonPropertyName("format")]
    public string? Format { get; set; } // Para HEADER (IMAGE, DOCUMENT, VIDEO)
}