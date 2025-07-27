using Contacts.Application.Abstractions;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Contacts.Application.UseCases.Commands.Handlers;

    public class AtualizarAvatarContatoCommandHandler : ICommandHandler<AtualizarAvatarContatoCommand>
{

    private readonly IMetaContactService _metaContactService;
    private readonly IContactRepository _contactRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AtualizarAvatarContatoCommandHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    public AtualizarAvatarContatoCommandHandler(IMetaContactService metaContactService,
        IFileStorageService fileStorageService,
        IHttpClientFactory httpClientFactory,
        ILogger<AtualizarAvatarContatoCommandHandler> logger)
    {
        _metaContactService = metaContactService;
        _fileStorageService = fileStorageService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task HandleAsync(AtualizarAvatarContatoCommand command, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetByWaIdAsync(command.WaId, cancellationToken);
        if (contato is null)
        {
            throw new NotFoundException($"Contato com o WaId '{command.WaId}' não encontrado.");
        }

        var avatarUrl = await _metaContactService.GetProfilePictureUrlAsync(command.WaId);
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            try
            {
                // Usa um HttpClient simples para baixar a imagem da URL temporária
                var httpClient = _httpClientFactory.CreateClient();
                var imageStream = await httpClient.GetStreamAsync(avatarUrl, cancellationToken);

                // Faz o upload para nosso S3 para obter uma URL permanente
                var fileName = $"avatar-{command.WaId}.jpg";
                var permanentAvatarUrl = await _fileStorageService.UploadAsync(imageStream, fileName, "image/jpeg");

                // Define a URL permanente no nosso agregado
                contato.DefinirAvatarUrl(permanentAvatarUrl);
            }
            catch (Exception ex)
            {
                // Logar o erro, mas não impedir a criação do contato se o download do avatar falhar.
                _logger.LogWarning(ex, "Falha ao baixar e salvar o avatar para o contato {ContatoId}", contato.Id);
            }
        }
    }

}

