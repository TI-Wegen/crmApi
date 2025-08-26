using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta;

public record MetaWebhookPayload
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("entry")] public List<EntryObject> Entry { get; set; }
}

public record EntryObject
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("changes")] public List<ChangeObject> Changes { get; set; }
}

public record ChangeObject
{
    [JsonPropertyName("field")] public string Field { get; set; }

    [JsonPropertyName("value")] public ValueObject Value { get; set; }
}

public record ValueObject
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; }

    [JsonPropertyName("metadata")] public MetadataObject Metadata { get; set; }

    [JsonPropertyName("contacts")] public List<ContactObject>? Contacts { get; set; }

    [JsonPropertyName("messages")] public List<MessageObject>? Messages { get; set; }

    [JsonPropertyName("statuses")] public List<StatusObject>? Statuses { get; set; }
    [JsonPropertyName("event")] public string? Event { get; set; }

    [JsonPropertyName("message_template_id")]
    public string? MessageTemplateId { get; set; }

    [JsonPropertyName("message_template_name")]
    public string? MessageTemplateName { get; set; }

    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public record MetadataObject
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; }

    [JsonPropertyName("phone_number_id")] public string PhoneNumberId { get; set; }
}

public record ContactObject
{
    [JsonPropertyName("profile")] public ProfileObject Profile { get; set; }

    [JsonPropertyName("wa_id")] public string WaId { get; set; }
}

public record ProfileObject
{
    [JsonPropertyName("name")] public string Name { get; set; }
}

public record MessageObject
{
    [JsonPropertyName("from")] public string From { get; set; }
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("timestamp")] public string Timestamp { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("text")] public TextObject? Text { get; set; }

    [JsonPropertyName("image")] public MediaObject? Image { get; set; }

    [JsonPropertyName("document")] public DocumentObject? Document { get; set; }

    [JsonPropertyName("audio")] public MediaObject? Audio { get; set; }

    [JsonPropertyName("interactive")] public InteractiveReplyPayload? Interactive { get; set; }
}

public record TextObject
{
    [JsonPropertyName("body")] public string Body { get; set; }
}

public record MediaObject
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("mime_type")] public string MimeType { get; set; }

    [JsonPropertyName("caption")] public string? Caption { get; init; }
}

public record StatusObject
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }
}

public record ButtonReplyPayload
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; }
}

public record InteractiveReplyPayload
{
    [JsonPropertyName("type")] public string Type { get; init; }

    [JsonPropertyName("button_reply")] public ButtonReplyPayload ButtonReply { get; init; }
}

public record DocumentObject
{
    [JsonPropertyName("id")] public string Id { get; init; }

    [JsonPropertyName("mime_type")] public string MimeType { get; init; }

    [JsonPropertyName("caption")] public string? Caption { get; init; }

    [JsonPropertyName("filename")] public string? Filename { get; init; }
}