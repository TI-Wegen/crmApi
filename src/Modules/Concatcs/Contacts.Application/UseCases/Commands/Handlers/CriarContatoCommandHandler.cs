namespace Contacts.Application.UseCases.Commands.Handlers;

using Contacts.Application.Abstractions;
using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using Microsoft.Extensions.Logging;

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
        //var waId = await _metaContactService.VerifyContactAndGetWaIdAsync(command.Telefone);
        //if (string.IsNullOrEmpty(waId))
        //{
        //    throw new DomainException("O número de telefone fornecido não é um usuário válido do WhatsApp.");
        //}

        // 2. Usar o método de fábrica do domínio
        var contato = Contato.Criar(command.Nome, command.Telefone, command.WaId);

        //var tempAvatarUrl = await _metaContactService.GetProfilePictureUrlAsync(contato.WaId);
        //if (!string.IsNullOrEmpty(tempAvatarUrl))
        //{
        //    try
        //    {
        //        // Usa um HttpClient simples para baixar a imagem da URL temporária
        //        var httpClient = _httpClientFactory.CreateClient();
        //        var imageStream = await httpClient.GetStreamAsync(tempAvatarUrl, cancellationToken);

        //        // Faz o upload para nosso S3 para obter uma URL permanente
        //        var fileName = $"avatar-{contato.WaId}.jpg";
        //        var permanentAvatarUrl = await _fileStorageService.UploadAsync(imageStream, fileName, "image/jpeg");

        //        // Define a URL permanente no nosso agregado
        //        contato.DefinirAvatarUrl(permanentAvatarUrl);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Logar o erro, mas não impedir a criação do contato se o download do avatar falhar.
        //        _logger.LogWarning(ex, "Falha ao baixar e salvar o avatar para o contato {ContatoId}", contato.Id);
        //    }
        //}


        await _contactRepository.AddAsync(contato, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contato.ToDto();
    }
}