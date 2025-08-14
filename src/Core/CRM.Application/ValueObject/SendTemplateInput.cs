namespace CRM.Application.ValueObject;

public class SendTemplateInput
{
    public SendTemplateInput(string to,
        string templateName,
        List<string> parameters,
        TemplateType type,
        string? documentUrl = null)
    {
        To = to;
        TemplateName = templateName;
        Parameters = parameters;
        Type = type;
        DocumentUrl = documentUrl;
    }

    public string To { get; }
    public string TemplateName { get; }
    public List<string> Parameters { get; }
    public TemplateType Type { get; }
    public string? DocumentUrl { get; set; } = null;
    public string Caption { get; set; } = string.Empty;

}

