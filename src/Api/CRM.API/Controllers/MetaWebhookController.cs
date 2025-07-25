using Amazon.Runtime.Internal;
using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Queries;
using Conversations.Application.Abstractions;
using Conversations.Application.UseCases.Commands;
using Conversations.Infrastructure.Services;
using CRM.API.Services;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Storage;
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
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string token)
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
                _logger.LogInformation("Trava não adquirida para {Telefone}. Requisição concorrente ignorada.", telefoneDoContato);
                return;
            }

            try
            {
                // O switch agora delega para o método especialista apropriado.
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
                        _logger.LogInformation("Tipo de mensagem '{MessageType}' recebido para {Telefone} e ignorado.", message.Type, telefoneDoContato);
                        break;
                }
            }
            finally
            {
                await _distributedLock.ReleaseLockAsync(lockKey);
            }
        }

        private async Task HandleTextMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            var telefoneDoContato = message.From;

            // A lógica de buffer pertence apenas a mensagens de texto.
            await _messageBuffer.AddToBufferAsync(telefoneDoContato, message);
            if (!await _messageBuffer.IsFirstProcessor(telefoneDoContato))
            {
                return;
            }
            await Task.Delay(TimeSpan.FromSeconds(3));
            var mensagensAgrupadas = (await _messageBuffer.ConsumeBufferAsync(telefoneDoContato)).ToList();
            if (!mensagensAgrupadas.Any()) return;

            var textoDaMensagem = string.Join(" ", mensagensAgrupadas.Select(m => WebhookMessageParser.ParseMessage(m)).Where(b => !string.IsNullOrEmpty(b)));
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var primeiroTimestampUnix = long.Parse(mensagensAgrupadas.First().Timestamp);
            var timestampMensagem = DateTime.UnixEpoch.AddSeconds(primeiroTimestampUnix).ToUniversalTime();

            // O resto da lógica de roteamento que já tínhamos...
            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);
            var botSession = isDeveloper ? await _botSessionCache.GetStateAsync(telefoneDoContato) : null;

            if (botSession is not null)
            {
                var processarRespostaCommand = new ProcessarRespostaDoMenuCommand(telefoneDoContato, textoDaMensagem, timestampMensagem);
                await _processarRespostaHandler.HandleAsync(processarRespostaCommand);
            }
            else
            {
                // Inicia um novo fluxo de bot
                Guid contatoId;
                var getContactQuery = new GetContactByTelefoneQuery(telefoneDoContato);
                var contatoDto = await _getContactByTelefoneHandler.HandleAsync(getContactQuery);
                if (contatoDto is null)
                {
                    var createContactCommand = new CriarContatoCommand(nomeDoContato, telefoneDoContato, waIdDoContato);
                    var novoContatoDto = await _criarContatoHandler.HandleAsync(createContactCommand);
                    contatoId = novoContatoDto.Id;
                }
                else { contatoId = contatoDto.Id; }

                var iniciarConversaCommand = new IniciarConversaCommand(contatoId, textoDaMensagem, Timestamp: timestampMensagem, IniciarComBot: isDeveloper);
                await _iniciarConversaHandler.HandleAsync(iniciarConversaCommand);
            }
        }

        private async Task HandleAudioMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            // Guardião de segurança
            if (message.Audio is null)
            {
                _logger.LogWarning("Mensagem do tipo 'audio' recebida, mas sem o objeto 'audio'. Ignorando.");
                return;
            }

            // 1. Baixa a mídia da Meta e faz o upload para nosso storage (S3/Minio). 
            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Audio.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Audio.Id);
                return;
            }

            var anexoUrl = await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            // 2. Usa nosso parser para obter o texto descritivo.
            var textoParaProcessar = WebhookMessageParser.ParseMessage(message);

            // 3. Extrai as outras informações necessárias.
            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var timestampUnix = long.Parse(message.Timestamp);
            var timestampMensagem = DateTime.UnixEpoch.AddSeconds(timestampUnix).ToUniversalTime();

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() && _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            // 4. REUTILIZA nosso fluxo de iniciar um novo atendimento, agora passando a URL do anexo.
            await IniciarNovoFluxoDeAtendimento(telefoneDoContato, 
                nomeDoContato, 
                textoParaProcessar, 
                timestampMensagem, 
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
            if (parts.Length == 3 && parts[0] == "rating" && Guid.TryParse(parts[1], out var atendimentoId) && int.TryParse(parts[2], out var nota))
            {
                var command = new RegistrarAvaliacaoCommand(atendimentoId, nota);
                await _registrarAvaliacaoHandler.HandleAsync(command);
            }
        }

        private async Task HandleImageMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            if (message.Image is null)
            {
                _logger.LogWarning("Mensagem do tipo 'image' recebida, mas sem o objeto 'image' no payload. Ignorando.");
                return;
            }
            // 1. Baixa a mídia da Meta e faz o upload para nosso storage (S3/Minio).
            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Image!.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Image!.Id);
                return;
            }

            var anexoUrl = await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            var textoParaProcessar = message.Image.Caption ?? "[Imagem Recebida]";
            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var timestampUnix = long.Parse(message.Timestamp);
            var timestampMensagem = DateTime.UnixEpoch.AddSeconds(timestampUnix).ToUniversalTime();

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() && _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            var botSession = await _botSessionCache.GetStateAsync(telefoneDoContato);

            bool isSessionValidAndFromToday = botSession is not null;
            if (isSessionValidAndFromToday)
            {
                var fusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var dataSessaoLocal = TimeZoneInfo.ConvertTimeFromUtc(botSession.LastActivityTimestamp, fusoHorarioBrasil).Date;
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
                    timestampMensagem, 
                    anexoUrl, 
                    isDeveloper, 
                    waIdDoContato);
            }
        }

        private async Task HandleDocumentMessageAsync(MessageObject message, ContactObject contactPayload)
        {
            // Guardião de segurança
            if (message.Document is null)
            {
                _logger.LogWarning("Mensagem do tipo 'document' recebida, mas sem o objeto 'document'. Ignorando.");
                return;
            }

            // 1. Baixa a mídia da Meta e faz o upload para nosso storage (S3/Minio). 
            var mediaFile = await _metaMediaService.DownloadMediaAsync(message.Document.Id);
            if (mediaFile is null)
            {
                _logger.LogError("Falha ao baixar a mídia com ID {MediaId} da Meta.", message.Document.Id);
                return;
            }

            var anexoUrl = await _fileStorageService.UploadAsync(mediaFile.Content, mediaFile.FileName, mediaFile.MimeType);

            // 2. Usa nosso parser para extrair o texto (legenda ou nome do arquivo).
            var textoParaProcessar = WebhookMessageParser.ParseMessage(message);

            // 3. Extrai as outras informações necessárias.
            var telefoneDoContato = message.From;
            var nomeDoContato = contactPayload.Profile.Name;
            var waIdDoContato = contactPayload.WaId;
            var timestampUnix = long.Parse(message.Timestamp);
            var timestampMensagem = DateTime.UnixEpoch.AddSeconds(timestampUnix).ToUniversalTime();

            var isDeveloper = _metaSettings.DeveloperPhoneNumbers.Any() && _metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato);

            // 4. REUTILIZA nosso fluxo de iniciar um novo atendimento, agora passando a URL do anexo.
            await IniciarNovoFluxoDeAtendimento(telefoneDoContato, nomeDoContato, textoParaProcessar, timestampMensagem, anexoUrl, isDeveloper, waIdDoContato);
        }
        private async Task IniciarNovoFluxoDeAtendimento(string telefoneDoContato, string nomeDoContato, string textoDaMensagem, DateTime timestamp, string? anexoUrl, bool isDeveloper, string waId)
        {
            Guid contatoId;
            var getContactQuery = new GetContactByTelefoneQuery(telefoneDoContato);
            var contatoDto = await _getContactByTelefoneHandler.HandleAsync(getContactQuery);
            if (contatoDto is null)
            {
                var createContactCommand = new CriarContatoCommand(nomeDoContato, telefoneDoContato,  waId);
                var novoContatoDto = await _criarContatoHandler.HandleAsync(createContactCommand);
                contatoId = novoContatoDto.Id;
            }
            else { contatoId = contatoDto.Id; }

            // O IniciarConversaCommand agora precisa aceitar o anexoUrl.
            var iniciarConversaCommand = new IniciarConversaCommand(
                ContatoId: contatoId,
                TextoDaMensagem: textoDaMensagem,
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

        private bool IsValidSignature(string payload, string signature)
        {
            // Lógica para validar o hash HMACSHA256 usando seu AppSecret
            // ...
            return true; // Implementação real da validação é necessária aqui
        }

    }


}

