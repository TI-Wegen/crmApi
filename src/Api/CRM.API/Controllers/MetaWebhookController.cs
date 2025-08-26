using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Queries;
using Conversations.Application.Abstractions;
using Conversations.Application.UseCases.Commands;
using CRM.API.Services;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Config.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Templates.Application.UseCases.Commands;
using Templates.Domain.Enuns;

namespace CRM.API.Controllers
{
    [Route("webhooks/[controller]")]
    [ApiController]
    public class MetaWebhookController : ControllerBase
    {
        private readonly MetaSettings _metaSettings;
        private readonly ICommandHandler<CriarContatoCommand, ContatoDto> _criarContatoHandler;
        private readonly ICommandHandler<IniciarConversaCommand, Guid> _iniciarConversaHandler;
        private readonly IQueryHandler<GetContactByTelefoneQuery, ContatoDto?> _getContactByTelefoneHandler;
        private readonly IBotSessionCache _botSessionCache;
        private readonly ICommandHandler<ProcessarRespostaDoMenuCommand> _processarRespostaHandler;
        private readonly ICommandHandler<AtualizarStatusTemplateCommand> _atualizarStatusHandler;
        private readonly ICommandHandler<RegistrarAvaliacaoCommand> _registrarAvaliacaoHandler;
        private readonly ICommandHandler<AtualizarAvatarContatoCommand> _atualizarAvatarHandler;
        private readonly IDistributedLock _distributedLock;
        private readonly IMessageBufferService _messageBuffer;
        private readonly IMetaMediaService _metaMediaService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MetaWebhookController> _logger;


        public MetaWebhookController(
            IOptions<MetaSettings> metaSettings,
            ICommandHandler<CriarContatoCommand, ContatoDto> criarContatoHandler,
            ICommandHandler<IniciarConversaCommand, Guid> iniciarConversaHandler,
            IQueryHandler<GetContactByTelefoneQuery, ContatoDto?> getContactByTelefoneHandler,
            IBotSessionCache botSessionCache,
            ICommandHandler<ProcessarRespostaDoMenuCommand> processarRespostaHandler,
            ICommandHandler<AtualizarStatusTemplateCommand> atualizarStatusHandler,
            ICommandHandler<RegistrarAvaliacaoCommand> registrarAvaliacaoHandler,
            IDistributedLock distributedLock,
            IMessageBufferService messageBuffer,
            IMetaMediaService metaMediaService,
            IFileStorageService fileStorageService,
            ILogger<MetaWebhookController> logger)

        {
            _metaSettings = metaSettings.Value;
            _criarContatoHandler = criarContatoHandler;
            _iniciarConversaHandler = iniciarConversaHandler;
            _getContactByTelefoneHandler = getContactByTelefoneHandler;
            _botSessionCache = botSessionCache;
            _processarRespostaHandler = processarRespostaHandler;
            _atualizarStatusHandler = atualizarStatusHandler;
            _registrarAvaliacaoHandler = registrarAvaliacaoHandler;
            _distributedLock = distributedLock;
            _messageBuffer = messageBuffer;
            _metaMediaService = metaMediaService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult VerifyWebhook
        (
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string token
        )
        {
            if (mode == "subscribe" && token == _metaSettings.VerifyToken)
            {
                Console.WriteLine("--> Webhook verificado com sucesso!");
                return Ok(challenge);
            }

            Console.WriteLine("--> Falha na verificação do Webhook.");
            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotification([FromBody] MetaWebhookPayload payload)
        {
            try
            {
                foreach (var change in payload.Entry.SelectMany(e => e.Changes))
                {
                    switch (change.Field)
                    {
                        case "messages":
                            await HandleMessageEvent(change.Value);
                            break;
                        case "message_template_status_update":
                            await HandleTemplateStatusUpdate(change.Value);
                            break;
                        case "profile":
                            await HandleProfileUpdate(change.Value);
                            break;
                        default:
                            Console.WriteLine($"--> Evento de webhook do tipo '{change.Field}' recebido e ignorado.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Erro crítico ao processar webhook: {ex.Message} \n {ex.StackTrace}");
            }

            return Ok();
        }

        private async Task HandleMessageEvent(ValueObject value)
        {
            var message = value?.Messages?.FirstOrDefault();
            var contactPayload = value?.Contacts?.FirstOrDefault();
            if (message is null || contactPayload is null) return;

            var telefoneDoContato = message.From;
            if (string.IsNullOrEmpty(telefoneDoContato)) return;

            var lockKey = $"lock:contato:{telefoneDoContato}";
            if (!await _distributedLock.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30)))
            {
                _logger.LogInformation("Trava não adquirida para {Telefone}. Requisição concorrente ignorada.",
                    telefoneDoContato);
                return;
            }

            try
            {
                switch (message.Type)
                {
                    case "text":
                        await HandleTextMessageAsync(message, contactPayload);
                        break;
                    case "interactive":
                        await HandleInteractiveMessageAsync(message, contactPayload);
                        break;
                    case "image":
                        await HandleImageMessageAsync(message, contactPayload);
                        break;
                    case "document":
                        await HandleDocumentMessageAsync(message, contactPayload);
                        break;
                    case "audio":
                        await HandleAudioMessageAsync(message, contactPayload);
                        break;
                    default:
                        _logger.LogInformation("Tipo de mensagem '{MessageType}' recebido para {Telefone} e ignorado.",
                            message.Type, telefoneDoContato);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Erro ao processar mensagem de {telefoneDoContato}: {e.Message}", e);
            }
            finally
            {
                await _distributedLock.ReleaseLockAsync(lockKey);
            }
        }

        private async Task HandleTextMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            var telefoneDoContato = message.From;
            var textoDaMensagem = WebhookMessageParser.ParseMessage(message);
            if (string.IsNullOrEmpty(textoDaMensagem)) return;

            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var dataHoraBrasil = ConvertTimestampBR(message.Timestamp);

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);
            var botSession = isDeveloper ? await _botSessionCache.GetStateAsync(telefoneDoContato) : null;

            Guid contatoId;
            var contatoDto =
                await _getContactByTelefoneHandler.HandleAsync(new GetContactByTelefoneQuery(telefoneDoContato));
            if (contatoDto is null)
            {
                var novoContatoDto =
                    await _criarContatoHandler.HandleAsync(new CriarContatoCommand(nomeDoContato, telefoneDoContato,
                        waIdDoContato));
                contatoId = novoContatoDto.Id;
            }
            else
            {
                contatoId = contatoDto.Id;
            }

            if (botSession != null)
            {
                await _processarRespostaHandler.HandleAsync(
                    new ProcessarRespostaDoMenuCommand(telefoneDoContato, textoDaMensagem, dataHoraBrasil));
            }
            else
            {
                foreach (var chunk in SplitMessage(textoDaMensagem))
                {
                    await _iniciarConversaHandler.HandleAsync(new IniciarConversaCommand(contatoId, chunk,
                        nomeDoContato, Timestamp: dataHoraBrasil, IniciarComBot: isDeveloper));
                }
            }
        }

        private IEnumerable<string> SplitMessage(string message, int chunkSize = 4000)
        {
            if (string.IsNullOrEmpty(message))
                yield break;

            for (int i = 0; i < message.Length; i += chunkSize)
                yield return message.Substring(i, Math.Min(chunkSize, message.Length - i));
        }

        private async Task HandleAudioMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Audio.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Audio.Id);
                return;
            }

            var anexoUrl =
                await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            var textoParaProcessar = WebhookMessageParser.ParseMessage(message);

            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var dataHoraBrasil = ConvertTimestampBR(message.Timestamp);

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() &&
                              _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            await IniciarNovoFluxoDeAtendimento(telefoneDoContato,
                nomeDoContato,
                textoParaProcessar,
                dataHoraBrasil,
                anexoUrl,
                isDeveloper,
                waIdDoContato
            );
        }

        private async Task HandleInteractiveMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            if (message.Interactive?.Type != "button_reply") return;

            var buttonReplyId = message.Interactive.ButtonReply.Id;
            var parts = buttonReplyId.Split('_');
            if (parts.Length == 3 && parts[0] == "rating" && Guid.TryParse(parts[1], out var atendimentoId) &&
                int.TryParse(parts[2], out var nota))
            {
                var command = new RegistrarAvaliacaoCommand(atendimentoId, nota);
                await _registrarAvaliacaoHandler.HandleAsync(command);
            }
        }

        private async Task HandleImageMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Image!.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Image!.Id);
                return;
            }

            var anexoUrl =
                await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            var textoParaProcessar = message.Image.Caption ?? "[Imagem Recebida]";
            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;

            var dataHoraBrasil = ConvertTimestampBR(message.Timestamp);

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() &&
                              _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            var botSession = await _botSessionCache.GetStateAsync(telefoneDoContato);

            bool isSessionValidAndFromToday = botSession is not null;
            if (isSessionValidAndFromToday)
            {
                var fusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var dataSessaoLocal =
                    TimeZoneInfo.ConvertTimeFromUtc(botSession.LastActivityTimestamp, fusoHorarioBrasil).Date;
                var dataAtualLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoHorarioBrasil).Date;
                if (dataSessaoLocal != dataAtualLocal)
                {
                    isSessionValidAndFromToday = false;
                    await _botSessionCache.DeleteStateAsync(telefoneDoContato);
                }
            }
            else
            {
                await IniciarNovoFluxoDeAtendimento(telefoneDoContato,
                    nomeDoContato,
                    textoParaProcessar,
                    dataHoraBrasil,
                    anexoUrl,
                    isDeveloper,
                    waIdDoContato);
            }
        }

        private async Task HandleDocumentMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            if (message.Document is null)
            {
                _logger.LogWarning("Mensagem do tipo 'document' recebida, mas sem o objeto 'document'. Ignorando.");
                return;
            }

            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Document.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Document.Id);
                return;
            }

            var anexoUrl =
                await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            var textoParaProcessar = WebhookMessageParser.ParseMessage(message);

            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var dataHoraBrasil = ConvertTimestampBR(message.Timestamp);

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() &&
                              _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            await IniciarNovoFluxoDeAtendimento(telefoneDoContato, nomeDoContato, textoParaProcessar, dataHoraBrasil,
                anexoUrl, isDeveloper, waIdDoContato);
        }

        private async Task IniciarNovoFluxoDeAtendimento(string telefoneDoContato, string nomeDoContato,
            string textoDaMensagem, DateTime timestamp, string? anexoUrl, bool isDeveloper, string waId)
        {
            Guid contatoId;
            var getContactQuery = new GetContactByTelefoneQuery(telefoneDoContato);
            var contatoDto = await _getContactByTelefoneHandler.HandleAsync(getContactQuery);
            if (contatoDto is null)
            {
                var createContactCommand = new CriarContatoCommand(nomeDoContato, telefoneDoContato, waId);
                var novoContatoDto = await _criarContatoHandler.HandleAsync(createContactCommand);
                contatoId = novoContatoDto.Id;
            }
            else
            {
                contatoId = contatoDto.Id;
            }

            var iniciarConversaCommand = new IniciarConversaCommand(
                ContatoId: contatoId,
                TextoDaMensagem: textoDaMensagem,
                ContatoNome: nomeDoContato,
                Timestamp: timestamp,
                AnexoUrl: anexoUrl,
                IniciarComBot: isDeveloper
            );
            await _iniciarConversaHandler.HandleAsync(iniciarConversaCommand);
        }

        private async Task HandleTemplateStatusUpdate(ValueObject value)
        {
            var novoStatus = ParseTemplateStatus(value.Event);
            if (novoStatus.HasValue && !string.IsNullOrEmpty(value.MessageTemplateName))
            {
                var command = new AtualizarStatusTemplateCommand(
                    value.MessageTemplateName,
                    novoStatus.Value,
                    value.Reason
                );
                await _atualizarStatusHandler.HandleAsync(command);
            }
        }

        private async Task HandleProfileUpdate(ValueObject value)
        {
            var contactPayload = value?.Contacts?.FirstOrDefault();
            if (contactPayload is null) return;

            var command = new AtualizarAvatarContatoCommand(contactPayload.WaId);
            await _atualizarAvatarHandler.HandleAsync(command); // Injete o novo handler
        }

        private TemplateStatus? ParseTemplateStatus(string? eventName)
        {
            return eventName?.ToUpper() switch
            {
                "APPROVED" => TemplateStatus.Aprovado,
                "REJECTED" => TemplateStatus.Rejeitado,
                // Adicione outros status como "PENDING" ou "PAUSED" se necessário
                _ => null
            };
        }

        private DateTime ConvertTimestampBR(string timestamo)
        {
            var primeiroTimestampUnix = long.Parse(timestamo);
            var utcDateTime = DateTimeOffset.FromUnixTimeSeconds(primeiroTimestampUnix).UtcDateTime;

            var fusoHorarioBr = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasil = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, fusoHorarioBr);

            return dataHoraBrasil;
        }
    }
}