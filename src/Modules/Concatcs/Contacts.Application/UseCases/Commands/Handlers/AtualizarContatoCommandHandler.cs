namespace Contacts.Application.UseCases.Commands.Handlers;


using Contacts.Domain.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class AtualizarContatoCommandHandler : ICommandHandler<AtualizarContatoCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarContatoCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtualizarContatoCommand command, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken);
        if (contato is null)
            throw new NotFoundException($"Contato com o Id '{command.ContactId}' não encontrado.");

        if (contato.Telefone != command.NovoTelefone)
        {
            var existingContact = await _contactRepository.GetByTelefoneAsync(command.NovoTelefone, cancellationToken);
            if (existingContact is not null && existingContact.Id != contato.Id)
            {
                throw new Exception($"O telefone '{command.NovoTelefone}' já está em uso por outro contato.");
            }
        }

        contato.Atualizar(command.NovoNome, command.NovoTelefone, command.NovasTags);

        await _contactRepository.UpdateAsync(contato); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}