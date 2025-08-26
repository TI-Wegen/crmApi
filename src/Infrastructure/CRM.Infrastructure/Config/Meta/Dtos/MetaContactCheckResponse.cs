using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Config.Meta.Dtos;

    public record MetaContactCheckResponse
{
    [JsonPropertyName("contacts")]
    public List<ContactCheckResult> Contacts { get; init; }
}

public record ContactCheckResult
{
    [JsonPropertyName("input")]
    public string Input { get; init; }
    
    [JsonPropertyName("status")]
    public string Status { get; init; }
    
    [JsonPropertyName("wa_id")]
    public string WaId { get; init; }
}