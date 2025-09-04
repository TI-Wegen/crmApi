using Conversations.Domain.Aggregates;

namespace Conversations.Application.Abstractions
{
    public interface IAtendimentoRepository
    {
        Task AddAsync(Atendimento atendimento, CancellationToken cancellationToken = default);
        Task<Atendimento> FindActiveByConversaIdAsync(Guid conversaId, CancellationToken cancellationToken = default);
        Task<Atendimento?> GetByIdAsync(Guid atendimentoId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Atendimento>> GetAtendimentosAtivosCriadosAntesDeAsync(DateTime dataLimite,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Atendimento>> GetAtendimentosEmAutoAtendimentoAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Atendimento>> GetLastTwoByConversaIdAsync(Guid conversaId,
            CancellationToken cancellationToken = default);

        Task AddTagAtendimento(Guid contactId, Guid tagId, CancellationToken cancellationToken);
    }
}