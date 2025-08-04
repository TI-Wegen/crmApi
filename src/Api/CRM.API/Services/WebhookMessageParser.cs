namespace CRM.API.Services;

using CRM.Infrastructure.Config.Meta;

public static class WebhookMessageParser
{    public static string ParseMessage(MessageObject message)
    {
        return message.Type.ToLower() switch
        {
            "text" => message.Text?.Body ?? string.Empty,
            "image" => message.Image?.Caption ?? "[Imagem recebida]",
            "audio" => "[Áudio recebido]",
            "video" => "[Vídeo recebido]",
            "document" => message.Document?.Caption ?? message.Document?.Filename ?? "[Documento recebido]",
            "sticker" => "[Sticker recebido]",
            "location" => "[Localização recebida]",
            _ => $"[Tipo de mensagem '{message.Type}' não suportado]"
        };
    }
}