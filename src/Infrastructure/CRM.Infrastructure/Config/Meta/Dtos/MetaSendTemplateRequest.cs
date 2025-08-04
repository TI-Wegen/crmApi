namespace CRM.Infrastructure.Config.Meta.Dtos;

using System.Text.Json.Serialization;
using System.Collections.Generic;


public record MetaSendTemplateRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; } = "whatsapp";

    [JsonPropertyName("to")]
    public string To { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; } = "template";

    [JsonPropertyName("template")]
    public TemplatePayload Template { get; init; }

    public MetaSendTemplateRequest(string to, string templateName, List<string> bodyParameters)
    {
        To = to;
        Template = new TemplatePayload(templateName, bodyParameters);
    }
}

public record TemplatePayload
{
    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("language")]
    public LanguagePayload Language { get; } = new("pt_BR"); 

    [JsonPropertyName("components")]
    public List<ComponentPayload> Components { get; init; }

    public TemplatePayload(string name, List<string> bodyParameters)
    {
        Name = name;
        Components = new List<ComponentPayload>
        {
            new ComponentPayload("body", bodyParameters.Select(p => new ParameterPayload("text", p)).ToList())
        };
    }
}

public record LanguagePayload([property: JsonPropertyName("code")] string Code);

public record ComponentPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("parameters")] List<ParameterPayload> Parameters
);

public record ParameterPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text
);