using Conversations.Domain.Exceptions;
using CRM.Domain.DomainEvents;

namespace Agents.Domain.Aggregates;

public class Setor : Entity
{
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }

    private Setor() { }

    public static Setor Criar(string nome, string? descricao = null)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do setor é obrigatório.");

        return new Setor
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Descricao = descricao
        };
    }
}