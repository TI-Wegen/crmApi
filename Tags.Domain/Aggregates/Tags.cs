using Agents.Domain.Aggregates;
using CRM.Domain.DomainEvents;
using Tags.Domain.Enum;

namespace Tags.Domain.Aggregates;

public class Tags : Entity
{
    public string Nome { get; set; }
    public string Cor { get; set; }
    public string? Descricao { get; set; }
    public TagsStatus Status { get; set; }
    public Guid AgenteId { get; set; }

    private Tags()
    {
    }

    public static Tags Criar(string nome, string cor, string descricao, Guid agentId)
    {
        var tags = new Tags
        {
            Nome = nome,
            Cor = cor,
            Descricao = descricao,
            AgenteId = agentId
        };
        return tags;
    }

    public void Atualizar(string nome, string cor, string descricao)
    {
        Nome = nome;
        Cor = cor;
        Descricao = descricao;
    }

    public void Inativar()
    {
        if (Status == TagsStatus.Inativo) return;

        Status = TagsStatus.Inativo;
    }
}