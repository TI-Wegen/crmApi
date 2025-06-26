namespace CRM.API.Services;

using CRM.API.Dtos.Meta;



public static class WebhookMessageParser
{
    // Este método converte um objeto de mensagem da Meta em um texto simples.
    public static string ParseMessage(MessageObject message)
    {
        return message.Type.ToLower() switch
        {
            "text" => message.Text?.Body ?? string.Empty,
            "image" => "[Imagem recebida]",
            "audio" => "[Áudio recebido]",
            "video" => "[Vídeo recebido]",
            "document" => "[Documento recebido]",
            "sticker" => "[Sticker recebido]",
            "location" => "[Localização recebida]",
            _ => $"[Tipo de mensagem '{message.Type}' não suportado]"
        };
    }
}