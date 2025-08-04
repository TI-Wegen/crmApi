namespace Agents.Domain.Aggregates;

using Agents.Domain.Enuns;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;

public class Agente : Entity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public AgenteStatus Status { get; private set; }
    public CargaDeTrabalho CargaDeTrabalho { get; private set; }

    private readonly List<Guid> _setorIds = new();
    public IReadOnlyCollection<Guid> SetorIds => _setorIds.AsReadOnly();

    private Agente() { }

    public static Agente Criar(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do agente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("O e-mail do agente é obrigatório.");

        return new Agente
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            Status = AgenteStatus.Offline, 
            CargaDeTrabalho = CargaDeTrabalho.Nenhuma() 
        };
    }

    public void AlterarStatus(AgenteStatus novoStatus)
    {
        Status = novoStatus;
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

        _setorIds.Clear();
        foreach (var setorId in novosSetorIds ?? new List<Guid>())
        {
            AtribuirASetor(setorId);
        }
    }

    public void Inativar()
    {
        if (CargaDeTrabalho.Valor > 0)
            throw new DomainException("Não é possível inativar um agente com conversas ativas.");

        if (Status == AgenteStatus.Inativo) return; 

        Status = AgenteStatus.Inativo;

    }

    public void DefinirSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
        {
            throw new DomainException("A senha deve ter no mínimo 8 caracteres.");
        }
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(senha);
    }

    public bool VerificarSenha(string senha)
    {
        return BCrypt.Net.BCrypt.Verify(senha, this.PasswordHash);
    }
}