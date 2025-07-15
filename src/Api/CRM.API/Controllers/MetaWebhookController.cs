using Amazon.Runtime.Internal;
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
        private readonly IDistributedLock _distributedLock;
        private readonly IMessageBufferService _messageBuffer;


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
            IMessageBufferService messageBuffer)

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
            // Ignora notificações de status de mensagem (ex: "sent", "delivered", "read") por enquanto.
            if (value?.Statuses is not null && value.Statuses.Any())
            {
                Console.WriteLine("--> Notificação de status de mensagem recebida e ignorada.");
                return;
            }


            var message = value?.Messages?.FirstOrDefault();
            var contactPayload = value?.Contacts?.FirstOrDefault();
            if (message is null || contactPayload is null) return;

            var telefoneDoContato = message.From;

            if (string.IsNullOrEmpty(telefoneDoContato))
            {
                Console.WriteLine("--> Mensagem recebida sem número de telefone do contato. Ignorando.");
                return;
            }
            var lockKey = $"lock:contato:{telefoneDoContato}";

            if (!await _distributedLock.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30)))
            {
                Console.WriteLine($"--> Trava não adquirida para {telefoneDoContato}. Requisição concorrente ignorada.");
                return;
            }

            try {

                if (message.Type == "interactive" && message.Interactive?.Type == "button_reply")
                {
                    var buttonReplyId = message.Interactive.ButtonReply.Id;

                    // Extrai os dados do ID do botão
                    var parts = buttonReplyId.Split('_');
                    if (parts.Length == 3 && parts[0] == "rating" && Guid.TryParse(parts[1], out var conversaId) && int.TryParse(parts[2], out var nota))
                    {
                        // Despacha o comando para registrar a avaliação
                        var command = new RegistrarAvaliacaoCommand(conversaId, nota);
                        await _registrarAvaliacaoHandler.HandleAsync(command); // Injete este novo handler
                    }
                }
                else if (message.Type == "text")
                {
                    // 2. Adiciona a mensagem atual ao buffer do Redis.
                    await _messageBuffer.AddToBufferAsync(telefoneDoContato, message);

                    // 3. Tenta se registrar como o "processador". Se falhar, outro processo já está cuidando disso.
                    if (!await _messageBuffer.IsFirstProcessor(telefoneDoContato))
                    {
                        Console.WriteLine($"--> Mensagem para {telefoneDoContato} adicionada ao buffer. Outro processo irá tratar.");
                        return;
                    }

                    // 4. Se chegamos aqui, somos os primeiros. Esperamos para agrupar mais mensagens.
                    await Task.Delay(TimeSpan.FromSeconds(3)); // A janela de 3 segundos do seu ADR.

                    // 5. Consome o buffer completo.
                    var mensagensAgrupadas = (await _messageBuffer.ConsumeBufferAsync(telefoneDoContato)).ToList();
                    if (!mensagensAgrupadas.Any()) return;

                    // 6. Concatena o texto de todas as mensagens para obter o contexto completo.
                    var textoDaMensagem = string.Join(" ", mensagensAgrupadas
                        .Select(m => WebhookMessageParser.ParseMessage(m)) // Usa nosso parser para lidar com diferentes tipos
                        .Where(b => !string.IsNullOrEmpty(b)));
        

                    // Filtro de segurança para desenvolvimento
                    if (_metaSettings.DeveloperPhoneNumbers.Any() && !_metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato))
                    {
                        Console.WriteLine($"--> Mensagem do número {telefoneDoContato} ignorada (não está na lista de desenvolvedores).");
                        return;
                    }

                    var nomeDoContato = contactPayload.Profile.Name;

                    // Roteia para o handler correto baseado na sessão do bot
                    var botSession = await _botSessionCache.GetStateAsync(telefoneDoContato);
                    if (botSession is null)
                    {
                        // Inicia um novo fluxo de bot
                        Guid contatoId;
                        var getContactQuery = new GetContactByTelefoneQuery(telefoneDoContato);
                        var contatoDto = await _getContactByTelefoneHandler.HandleAsync(getContactQuery);
                        if (contatoDto is null)
                        {
                            var createContactCommand = new CriarContatoCommand(nomeDoContato, telefoneDoContato);
                            var novoContatoDto = await _criarContatoHandler.HandleAsync(createContactCommand);
                            contatoId = novoContatoDto.Id;
                        }
                        else { contatoId = contatoDto.Id; }

                        var iniciarConversaCommand = new IniciarConversaCommand(contatoId, textoDaMensagem);
                        await _iniciarConversaHandler.HandleAsync(iniciarConversaCommand);
                    }
                    else
                    {
                        // Continua um fluxo de bot existente
                        var processarRespostaCommand = new ProcessarRespostaDoMenuCommand(telefoneDoContato, textoDaMensagem);
                        await _processarRespostaHandler.HandleAsync(processarRespostaCommand);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Erro ao adquirir a trava para {telefoneDoContato}: {ex.Message}");
                return;
            }
            finally
            {
                // Garante que a trava será liberada mesmo se ocorrer um erro
                await _distributedLock.ReleaseLockAsync(lockKey);
            }


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

