namespace Conversations.Application.UseCases.Commands;

public record AtualizarAgenteCommand(string Nome, List<Guid> SetorIds);
