namespace Contacts.Domain.Aggregates;

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
    public string WaId { get; private set; }
    public string? AvatarUrl { get; private set; }


    private Contato() { }

    public static Contato Criar(string nome, string telefone, string waId)
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
    
        if (Status == ContatoStatus.Inativo) return; 

        AlterarStatus(ContatoStatus.Inativo);

    }
}