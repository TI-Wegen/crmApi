using Contacts.Application.Repositories;
using Conversations.Application.Repositories;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class ResolverAtendimentoCommandHandler : ICommandHandler<ResolverAtendimentoCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IBotSessionCache _botSessionCache;

    public ResolverAtendimentoCommandHandler(IAtendimentoRepository atendimentoRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IBotSessionCache botSessionCache,
        IContactRepository contactRepository,
        IConversationRepository conversationRepository
    )
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _botSessionCache = botSessionCache;
        _contactRepository = contactRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task HandleAsync(ResolverAtendimentoCommand command, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimento is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        var agenteIdLogado = _userContext.GetCurrentUserId();
        if (agenteIdLogado is null)
        {
            throw new UnauthorizedAccessException("Usuário não está autenticado ou não possui um agente associado.");
        }

        atendimento.Resolver(agenteIdLogado);

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{atendimento.ConversaId}' não encontrada.");

        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is null) throw new NotFoundException($"Contato com o Id '{conversa.ContatoId}' não encontrado.");

        await _botSessionCache.DeleteStateAsync(contato.Telefone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}