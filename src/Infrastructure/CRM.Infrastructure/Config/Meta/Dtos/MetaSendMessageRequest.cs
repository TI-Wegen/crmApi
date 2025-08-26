using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos
{
    public record MetaSendMessageRequest
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; } = "whatsapp";

        [JsonPropertyName("to")] public string To { get; init; }

        [JsonPropertyName("type")] public string Type { get; } = "text";

        [JsonPropertyName("text")] public TextPayload Text { get; init; }

        public MetaSendMessageRequest(string to, string textBody)
        {
            To = to;
            Text = new TextPayload(textBody);
        }
    }

    public record TextPayload
    {
        [JsonPropertyName("body")] public string Body { get; init; }

        public TextPayload(string body)
        {
            Body = body;
        }
    }
}