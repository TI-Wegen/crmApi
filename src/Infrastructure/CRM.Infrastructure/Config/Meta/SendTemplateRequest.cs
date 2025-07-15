namespace CRM.Infrastructure.Config.Meta;

public record SendTemplateRequest(string TemplateName, List<string> BodyParameters);
