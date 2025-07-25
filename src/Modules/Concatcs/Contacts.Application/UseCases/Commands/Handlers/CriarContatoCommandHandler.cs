namespace Contacts.Application.UseCases.Commands.Handlers;


using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using CRM.Application.Interfaces;

public class CriarContatoCommandHandler : ICommandHandler<CriarContatoCommand, ContatoDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarContatoCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContatoDto> HandleAsync(CriarContatoCommand command, CancellationToken cancellationToken)
    {
        // 1. Validação da Aplicação: verificar se o telefone já existe
        var existingContact = await _contactRepository.GetByTelefoneAsync(command.Telefone, cancellationToken);
        if (existingContact is not null)
        {
            throw new Exception($"Já existe um contato com o telefone '{command.Telefone}'.");
        }

        // 2. Usar o método de fábrica do domínio
        var contato = Contato.Criar(command.Nome, command.Telefone, command.WaId);

        // 3. Adicionar ao repositório
        await _contactRepository.AddAsync(contato, cancellationToken);

        // 4. Salvar as alterações
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Mapear para DTO e retornar
        return contato.ToDto();
    }
}