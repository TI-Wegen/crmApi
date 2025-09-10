using Contacts.Application.Abstractions;
using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Contacts.Application.UseCases.Commands.Handlers;

public class CriarContatoCommandHandler : ICommandHandler<CriarContatoCommand, ContatoDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaContactService _metaContactService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CriarContatoCommandHandler> _logger;

    public CriarContatoCommandHandler(IContactRepository contactRepository,
        IUnitOfWork unitOfWork, IMetaContactService metaContactService,
        IFileStorageService fileStorageService,
        IHttpClientFactory httpClientFactory,
        ILogger<CriarContatoCommandHandler> logger
    )
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _metaContactService = metaContactService;
        _fileStorageService = fileStorageService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ContatoDto> HandleAsync(CriarContatoCommand command, CancellationToken cancellationToken)
    {
         var existingContact = await _contactRepository.GetByTelefoneAsync(command.Telefone, cancellationToken);
        if (existingContact is not null)
        {
            throw new Exception($"Já existe um contato com o telefone '{command.Telefone}'.");
        }
        
        var contato = Contato.Criar(command.Nome, command.Telefone, command.WaId ??  null);
        
        await _contactRepository.AddAsync(contato, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contato.ToDto();
    }

}
        