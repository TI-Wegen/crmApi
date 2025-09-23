using Conversations.Domain.Entities;

namespace Conversations.Application.Repository;

public interface IMensagemRepository
{
    Task UpdateAsync(Mensagem mensagem, CancellationToken cancellationToken = default);

    Task<Mensagem?> FindMessageByExternalIdAsync(string externalId,
        CancellationToken cancellationToken = default);
}