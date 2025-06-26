using System.Text.Json.Serialization;

namespace CRM.API.Dtos.Meta;

// Nível 1: O objeto raiz do webhook
public record MetaWebhookPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("entry")]
    public List<EntryObject> Entry { get; set; }
}

// Nível 2: Cada "entry" contém um conjunto de alterações
public record EntryObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("changes")]
    public List<ChangeObject> Changes { get; set; }
}

// Nível 3: Cada "change" tem um campo e um valor
public record ChangeObject
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("value")]
    public ValueObject Value { get; set; }
}

// Nível 4: O "value" contém os dados principais (mensagens, contatos, status)
public record ValueObject
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; }

    [JsonPropertyName("metadata")]
    public MetadataObject Metadata { get; set; }

    [JsonPropertyName("contacts")]
    public List<ContactObject>? Contacts { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageObject>? Messages { get; set; }

    [JsonPropertyName("statuses")]
    public List<StatusObject>? Statuses { get; set; }
}

// --- Objetos de Dados Aninhados ---

public record MetadataObject
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; }

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; }
}

public record ContactObject
{
    [JsonPropertyName("profile")]
    public ProfileObject Profile { get; set; }

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; }
}

public record ProfileObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public record MessageObject
{
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public TextObject? Text { get; set; }

    [JsonPropertyName("image")]
    public MediaObject? Image { get; set; }

    // Adicione outros tipos de mídia aqui se necessário (audio, video, etc.)
}

public record TextObject
{
    [JsonPropertyName("body")]
    public string Body { get; set; }
}

public record MediaObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; }
}

public record StatusObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    // ... outras propriedades de status ...
}