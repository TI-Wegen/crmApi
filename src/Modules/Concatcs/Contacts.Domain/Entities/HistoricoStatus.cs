namespace Contacts.Domain.Entities;

using Contacts.Domain.Enums;
using CRM.Domain.DomainEvents;

public class HistoricoStatus : Entity
{
    public ContatoStatus Status { get; private set; }
    public DateTime DataDeAlteracao { get; private set; }

    internal HistoricoStatus(ContatoStatus status, DateTime dataDeAlteracao)
    {
        Id = Guid.NewGuid();
        Status = status;
        DataDeAlteracao = dataDeAlteracao;
    }
}