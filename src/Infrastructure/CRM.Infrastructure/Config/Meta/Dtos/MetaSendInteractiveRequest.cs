using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos;

public record MetaSendInteractiveRequest(
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("interactive")]
    InteractivePayload Interactive
)
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct => "whatsapp";

    [JsonPropertyName("recipient_type")] public string RecipientType => "individual";
    [JsonPropertyName("type")] public string Type => "interactive";
}

public record InteractivePayload(
    [property: JsonPropertyName("body")] InteractiveBodyPayload Body,
    [property: JsonPropertyName("action")] ActionPayload Action,
    [property: JsonPropertyName("header")] HeaderPayload? Header = null,
    [property: JsonPropertyName("footer")] TextObject? Footer = null
)
{
    [JsonPropertyName("type")] public string Type => "button";
}

public record InteractiveBodyPayload(
    [property: JsonPropertyName("text")] string Text
);

public record HeaderPayload(
    [property: JsonPropertyName("text")] string Text
)
{
    [JsonPropertyName("type")] public string Type => "text";
}

public record ActionPayload(
    [property: JsonPropertyName("buttons")]
    List<ButtonPayload> Buttons
);

public record ButtonPayload(
    [property: JsonPropertyName("reply")] ReplyPayload Reply
)
{
    [JsonPropertyName("type")] public string Type => "reply";
}

public record ReplyPayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title
);

public record TextObject(
    [property: JsonPropertyName("body")] string Body
);