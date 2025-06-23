namespace Agents.Domain.Aggregates;

using Agents.Domain.Enuns;
using Conversations.Domain.Exceptions;
using CRM.Domain.DomainEvents;

public class Agente : Entity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public AgenteStatus Status { get; private set; }
    public CargaDeTrabalho CargaDeTrabalho { get; private set; }

    private readonly List<Guid> _setorIds = new();
    public IReadOnlyCollection<Guid> SetorIds => _setorIds.AsReadOnly();

    private Agente() { } // Construtor privado para o EF Core e para forçar o uso do método de fábrica

    public static Agente Criar(string nome, string email)
    {
        // Validações (invariantes) para garantir um estado inicial consistente
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do agente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("O e-mail do agente é obrigatório.");

        return new Agente
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Status = AgenteStatus.Offline, // Um agente sempre começa como Offline
            CargaDeTrabalho = CargaDeTrabalho.Nenhuma() // E com carga de trabalho zerada 
        };
    }

    public void AlterarStatus(AgenteStatus novoStatus)
    {
        // Poderíamos ter regras aqui, ex: não pode ficar Online se não pertencer a nenhum setor.
        Status = novoStatus;
        // Disparar evento de domínio: AgenteStatusAlteradoEvent
    }

    public void AtribuirASetor(Guid setorId)
    {
        if (setorId == Guid.Empty) return;
        if (!_setorIds.Contains(setorId))
        {
            _setorIds.Add(setorId);
        }
    }

    public void IncrementarCarga()
    {
        CargaDeTrabalho = CargaDeTrabalho.Incrementar();
    }

    public void DecrementarCarga()
    {
        CargaDeTrabalho = CargaDeTrabalho.Decrementar();
    }

    public void Atualizar(string novoNome, List<Guid> novosSetorIds)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome do agente não pode ser vazio.");

        Nome = novoNome;

        // Atualiza a lista de setores
        _setorIds.Clear();
        foreach (var setorId in novosSetorIds ?? new List<Guid>())
        {
            AtribuirASetor(setorId);
        }

        // Opcional: Disparar um evento de domínio
        // AddDomainEvent(new AgenteAtualizadoEvent(this.Id));
    }
}