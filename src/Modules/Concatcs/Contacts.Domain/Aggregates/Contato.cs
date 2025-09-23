using Contacts.Domain.Entities;
using Contacts.Domain.Enums;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;

namespace Contacts.Domain.Aggregates;

public class Contato : Entity
{
    public string Nome { get; private set; }
    public string Telefone { get; private set; }
    public ContatoStatus Status { get; private set; }

    private readonly List<HistoricoStatus> _historicoStatus = new();
    public IReadOnlyCollection<HistoricoStatus> HistoricoStatus => _historicoStatus.AsReadOnly();
    public string? WaId { get; private set; }
    public string? AvatarUrl { get; private set; }

    private Contato()
    {
    }

    public static Contato Criar(string nome, string telefone, string? waId)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do contato é obrigatório.");
        if (string.IsNullOrWhiteSpace(telefone)) throw new DomainException("O telefone do contato é obrigatório.");

        var contato = new Contato
        {
            Nome = nome,
            Telefone = telefone,
            Status = ContatoStatus.Novo,
            WaId = waId,
        };

        contato.AdicionarEntradaNoHistorico(ContatoStatus.Novo);
        return contato;
    }

    public void DefinirAvatarUrl(string url)
    {
        AvatarUrl = url;
    }

    public void AlterarStatus(ContatoStatus novoStatus)
    {
        if (Status == novoStatus) return;

        Status = novoStatus;
        AdicionarEntradaNoHistorico(novoStatus);
    }

    private void AdicionarEntradaNoHistorico(ContatoStatus status)
    {
        var novaEntrada = new HistoricoStatus(status, DateTime.UtcNow);
        _historicoStatus.Add(novaEntrada);
    }

    public void Atualizar(string novoNome, string novoTelefone, Guid? tag = null)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome do contato não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(novoTelefone))
            throw new DomainException("O telefone do contato não pode ser vazio.");

        Nome = novoNome;
        Telefone = novoTelefone;
    }

    public void Inativar()
    {
        if (Status == ContatoStatus.Inativo) return;

        AlterarStatus(ContatoStatus.Inativo);
    }
}