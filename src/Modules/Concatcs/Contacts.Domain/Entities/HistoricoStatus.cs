namespace Contacts.Domain.Entities;

// Em Modules/Contacts/Domain/Entities/
using Contacts.Domain.Enums;
using CRM.Domain.DomainEvents;

public class HistoricoStatus : Entity
{
    public ContatoStatus Status { get; private set; }
    public DateTime DataDeAlteracao { get; private set; }

    // Construtor 'internal' para garantir que só o agregado 'Contato' possa criá-lo.
    internal HistoricoStatus(ContatoStatus status, DateTime dataDeAlteracao)
    {
        Id = Guid.NewGuid();
        Status = status;
        DataDeAlteracao = dataDeAlteracao;
    }
}