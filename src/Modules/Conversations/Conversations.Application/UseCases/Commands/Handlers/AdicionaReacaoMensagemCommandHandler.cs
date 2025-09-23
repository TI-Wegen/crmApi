using Conversations.Application.Repositories;
using Conversations.Domain.Entities;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AdicionaReacaoMensagemCommandHandler : ICommandHandler<AdicionaReacaoMensagemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMensagemRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;


    public AdicionaReacaoMensagemCommandHandler(IUnitOfWork unitOfWork, IMensagemRepository messageRepository,
        IConversationRepository conversationRepository)
    {
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task HandleAsync(AdicionaReacaoMensagemCommand command, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.FindMessageByExternalIdAsync(command.MessageId, cancellationToken);

        if (message is null)
        {
            throw new Exception("Mensagem não encontrada");
        }

        message.SetReacaoMensagem(command.Emoji);
        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
    }
}