using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AddTagCommandHandler : ICommandHandler<AddTagCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAtendimentoRepository _atendimentoRepository;

    public AddTagCommandHandler(IAtendimentoRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _atendimentoRepository = atendimentoRepository;
    }
    public Task HandleAsync(AddTagCommand command, CancellationToken cancellationToken = default)
    {
        return _atendimentoRepository.AddTagAtendimento(command.ContactId, command.TagId, cancellationToken);
    }
}