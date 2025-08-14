namespace CRM.Infrastructure.Config.Meta.Dtos;

using CRM.Application.ValueObject;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


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

    public MetaSendTemplateRequest(SendTemplateInput input)
    {
        To = input.To;
        Template = new TemplatePayload(input.TemplateName, input.Parameters, input.DocumentUrl, input.Type, input.Caption);
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



    public TemplatePayload(string name, List<string> bodyParameters, string? mediaUrl, TemplateType type, string caption)
    {
        Name = name;
        Components = new List<ComponentPayload>();

        // Add header component for document templates
        if (type == TemplateType.Document && !string.IsNullOrEmpty(mediaUrl))
        {
            Components.Add(new ComponentPayload(
                "header",
                new List<ParameterPayload>
                {
                ParameterPayload.CreateDocument(mediaUrl, "arquivo.pdf", caption)
                }
            ));
        }

        // Add body component with parameters (this was missing!)
        if (bodyParameters != null && bodyParameters.Count > 0)
        {
            Components.Add(new ComponentPayload(
                "body",
                bodyParameters.Select(param => new ParameterPayload("text", param)).ToList()
            ));
        }
    }
}

public record LanguagePayload([property: JsonPropertyName("code")] string Code);

public record ComponentPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("parameters")] List<ParameterPayload> Parameters


);

public record ParameterPayload
{
    [JsonPropertyName("type")] public string Type { get; init; }
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("document")] public DocumentPayload Document { get; init; }

    public ParameterPayload(string type, string text)
    {
        Type = type;
        Text = text;
    }

    private ParameterPayload(string type, DocumentPayload document)
    {
        Type = type;
        Document = document;
    }

    public static ParameterPayload CreateDocument(string url, string filename, string? caption)
        => new ParameterPayload("document", new DocumentPayload(url, filename, caption));
}


