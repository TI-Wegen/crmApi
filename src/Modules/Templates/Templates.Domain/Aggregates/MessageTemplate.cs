using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;
using Templates.Domain.Enuns;

namespace Templates.Domain.Aggregates;

public class MessageTemplate : Entity
{

    public string Name { get; private set; }
    public string Language { get; private set; }
    public string Body { get; private set; }
    public string? Description { get; private set; }

    public TemplateStatus Status { get; private set; }
    public string? MotivoRejeicao { get; private set; }
    private MessageTemplate() { }

    public static MessageTemplate Criar(string name, string language, string body, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do template é obrigatório.");
        if (string.IsNullOrWhiteSpace(language))
            throw new DomainException("O idioma do template é obrigatório.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("O corpo do template é obrigatório.");

        return new MessageTemplate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Language = language.Trim(),
            Body = body,
            Description = description,
            Status = TemplateStatus.Pendente
        };
    }


    public void AtualizarStatus(TemplateStatus novoStatus, string? motivo = null)
    {
        Status = novoStatus;
        MotivoRejeicao = motivo;
    }
}