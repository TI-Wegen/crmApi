using CRM.Domain.DomainEvents;
using Tags.Domain.Enum;

namespace Tags.Domain.Aggregates;

public class Tags : Entity
{
    public string Nome { get; set; }
    public string Cor { get; set; }
    public string? Descricao { get; set; }
    public TagsStatus Status { get; set; }

    private Tags()
    {
        
    }

    public static Tags Criar(string nome, string cor, string descricao)
    {
        var tags = new Tags
        {
            Nome = nome,
            Cor = cor,
            Descricao = descricao
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