namespace Contacts.Application.UseCases.Commands.Handlers;

using Contacts.Domain.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class InativarContatoCommandHandler : ICommandHandler<InativarContatoCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InativarContatoCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(InativarContatoCommand command, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken);
        if (contato is null)
            throw new NotFoundException($"Contato com o Id '{command.ContactId}' não encontrado.");

        contato.Inativar();

        await _contactRepository.UpdateAsync(contato);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}