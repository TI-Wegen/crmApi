namespace Contacts.Domain.Aggregates;

// Em Modules/Contacts/Domain/Aggregates/
using Contacts.Domain.Entities;
using Contacts.Domain.Enums;
using Contacts.Domain.ValueObjects;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;

public class Contato : Entity
{
    public string Nome { get; private set; }
    public string Telefone { get; private set; }
    public ContatoStatus Status { get; private set; }

    private readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    private readonly List<HistoricoStatus> _historicoStatus = new();
    public IReadOnlyCollection<HistoricoStatus> HistoricoStatus => _historicoStatus.AsReadOnly();

    private Contato() { }

    public static Contato Criar(string nome, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do contato é obrigatório.");
        if (string.IsNullOrWhiteSpace(telefone)) throw new DomainException("O telefone do contato é obrigatório.");

        var contato = new Contato
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Telefone = telefone,
            Status = ContatoStatus.Novo, 
        };

        // Adiciona o primeiro status ao histórico
        contato.AdicionarEntradaNoHistorico(ContatoStatus.Novo);
        return contato;
    }

    public void AlterarStatus(ContatoStatus novoStatus)
    {
        if (Status == novoStatus) return; // Nenhuma alteração necessária

        Status = novoStatus;
        AdicionarEntradaNoHistorico(novoStatus);

        // Disparar evento de domínio: ContatoStatusAlteradoEvent
    }

    public void AdicionarTag(string textoDaTag)
    {
        if (string.IsNullOrWhiteSpace(textoDaTag)) return;

        var novaTag = new Tag(textoDaTag.Trim());
        if (!_tags.Contains(novaTag))
        {
            _tags.Add(novaTag);
        }
    }

    private void AdicionarEntradaNoHistorico(ContatoStatus status)
    {
        var novaEntrada = new HistoricoStatus(status, DateTime.UtcNow);
        _historicoStatus.Add(novaEntrada);
    }

    public void Atualizar(string novoNome, string novoTelefone, List<string> novasTags)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome do contato não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(novoTelefone))
            throw new DomainException("O telefone do contato não pode ser vazio.");

        Nome = novoNome;
        Telefone = novoTelefone;

        _tags.Clear();
        foreach (var textoTag in novasTags ?? new List<string>())
        {
            AdicionarTag(textoTag);
        }

    }

    public void Inativar()
    {
    
        if (Status == ContatoStatus.Inativo) return; // Nenhuma ação necessária

        // Reutilizamos o método que já criamos para alterar o status e registrar no histórico!
        AlterarStatus(ContatoStatus.Inativo);

    }
}