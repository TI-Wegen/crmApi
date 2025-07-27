using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos;

    public record MetaContactCheckResponse
{
    [JsonPropertyName("contacts")]
    public List<ContactCheckResult> Contacts { get; init; }
}

public record ContactCheckResult
{
    // O número de telefone que nós enviamos na requisição.
    [JsonPropertyName("input")]
    public string Input { get; init; }

    // O status da verificação, que pode ser "valid" ou "invalid".
    [JsonPropertyName("status")]
    public string Status { get; init; }

    // O ID do WhatsApp (wa_id) do contato, se o status for "valid".
    [JsonPropertyName("wa_id")]
    public string WaId { get; init; }
}