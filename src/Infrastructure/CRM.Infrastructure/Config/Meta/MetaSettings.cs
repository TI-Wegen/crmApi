namespace CRM.Infrastructure.Config.Meta;

public class MetaSettings
{
    public string VerifyToken { get; set; }
    public string AppSecret { get; set; }
    public string BaseUrl { get; set; }
    public string MetaApiVersion { get; set; }
    public string WhatsAppBusinessAccountId { get; set; }
    public string WhatsAppBusinessPhoneNumberId { get; set; }
    public string WhatsAppBusinessPhoneNumber { get; set; }
    public string AccessToken { get; set; }
    public List<string> DeveloperPhoneNumbers { get; set; } = new();

}