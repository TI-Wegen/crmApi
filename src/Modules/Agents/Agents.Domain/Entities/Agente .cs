using Agents.Domain.Enuns;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;

namespace Agents.Domain.Entities;

public class Agente : Entity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public AgenteStatus Status { get; private set; }
    public Guid SetorId { get; private set; }

    private Agente()
    {
    }

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
        };
    }

    public void Atualizar(string novoNome, List<Guid> novosSetorIds)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome do agente não pode ser vazio.");

        Nome = novoNome;
    }

    public void Inativar()
    {
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
