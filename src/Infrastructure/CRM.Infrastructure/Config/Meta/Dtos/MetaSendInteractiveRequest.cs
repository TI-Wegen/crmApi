using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CRM.Infrastructure.Config.Meta.Dtos;

// --- DTOs para Envio de Mensagem Interativa com Botões (Versão Final) ---

/// <summary>
/// O DTO raiz que representa o corpo da requisição para enviar uma mensagem interativa.
/// </summary>
public record MetaSendInteractiveRequest(
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("interactive")] InteractivePayload Interactive
)
{
    [JsonPropertyName("messaging_product")] public string MessagingProduct => "whatsapp";
    [JsonPropertyName("recipient_type")] public string RecipientType => "individual";
    [JsonPropertyName("type")] public string Type => "interactive";
}

/// <summary>
/// O contêiner para a parte interativa, incluindo cabeçalho, corpo, rodapé e ações.
/// </summary>
public record InteractivePayload(
    [property: JsonPropertyName("body")] InteractiveBodyPayload Body, // MODIFICADO: Usa o novo DTO
    [property: JsonPropertyName("action")] ActionPayload Action,
    [property: JsonPropertyName("header")] HeaderPayload? Header = null,
    [property: JsonPropertyName("footer")] TextObject? Footer = null
)
{
    [JsonPropertyName("type")]
    public string Type => "button";
}

public record InteractiveBodyPayload(
    [property: JsonPropertyName("text")] string Text
);


/// <summary>
/// O contêiner para o cabeçalho (exemplo para um cabeçalho de texto).
/// </summary>
public record HeaderPayload(
    [property: JsonPropertyName("text")] string Text
)
{
    [JsonPropertyName("type")]
    public string Type => "text";
}

/// <summary>
/// O contêiner para a lista de botões.
/// </summary>
public record ActionPayload(
    [property: JsonPropertyName("buttons")] List<ButtonPayload> Buttons
);

/// <summary>
/// O contêiner para cada botão individual.
/// </summary>
public record ButtonPayload(
    [property: JsonPropertyName("reply")] ReplyPayload Reply
)
{
    [JsonPropertyName("type")]
    public string Type => "reply";
}

/// <summary>
/// Os detalhes do botão de resposta, com seu ID e título.
/// </summary>
public record ReplyPayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title
);

/// <summary>
/// O objeto de texto que pode ser usado no body e no footer.
/// </summary>
public record TextObject(
    [property: JsonPropertyName("body")] string Body
);