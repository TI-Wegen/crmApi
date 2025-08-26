using Contacts.Application.Abstractions;
using Contacts.Domain.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;

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
                var httpClient = _httpClientFactory.CreateClient();
                var imageStream = await httpClient.GetStreamAsync(avatarUrl, cancellationToken);

                var fileName = $"avatar-{command.WaId}.jpg";
                var permanentAvatarUrl = await _fileStorageService.UploadAsync(imageStream, fileName, "image/jpeg");

                contato.DefinirAvatarUrl(permanentAvatarUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao baixar e salvar o avatar para o contato {ContatoId}", contato.Id);
            }
        }
    }
}