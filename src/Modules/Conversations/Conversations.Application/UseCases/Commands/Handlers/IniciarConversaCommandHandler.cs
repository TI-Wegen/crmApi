using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

  public class IniciarConversaCommandHandler : ICommandHandler<IniciarConversaCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IniciarConversaCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        // 1. Criamos os objetos de domínio a partir dos dados do comando
        var remetente = Remetente.Cliente();
        var primeiraMensagem = new Mensagem(command.TextoDaPrimeiraMensagem, remetente, command.AnexoUrl);

        // 2. Usamos o método de fábrica do nosso agregado para criar a conversa
        var novaConversa = Conversa.Iniciar(command.ContatoId, primeiraMensagem);

        // 3. Adicionamos a nova conversa ao repositório
        await _conversationRepository.AddAsync(novaConversa, cancellationToken);

        // 4. Salvamos as mudanças no banco de dados
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Retornamos o ID da conversa recém-criada
        return novaConversa.Id;
    }
}