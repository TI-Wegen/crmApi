using Amazon.Runtime.Internal;
using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Queries;
using Conversations.Application.Abstractions;
using Conversations.Application.UseCases.Commands;
using Conversations.Domain.Enuns;
using CRM.API.Dtos.Meta;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Config.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;


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

        public MetaWebhookController(
            IOptions<MetaSettings> metaSettings,
            ICommandHandler<CriarContatoCommand, ContatoDto> criarContatoHandler,
            ICommandHandler<IniciarConversaCommand, Guid> iniciarConversaHandler,
            IQueryHandler<GetContactByTelefoneQuery, ContatoDto?> getContactByTelefoneHandler,
            IBotSessionCache botSessionCache,
            ICommandHandler<ProcessarRespostaDoMenuCommand> processarRespostaHandler
            )

        {
            _metaSettings = metaSettings.Value;
            _criarContatoHandler = criarContatoHandler;
            _iniciarConversaHandler = iniciarConversaHandler;
            _getContactByTelefoneHandler = getContactByTelefoneHandler;
            _botSessionCache = botSessionCache;
            _processarRespostaHandler = processarRespostaHandler;
        }

        // Endpoint para a verificação do Webhook (executado uma vez)
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
            // NOTA: A validação de assinatura (X-Hub-Signature-256) deve ser reativada para produção.
            try
            {
                var change = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault();
                if (change?.Field != "messages") return Ok("Evento ignorado.");


                var status = change.Value?.Statuses?.FirstOrDefault();
                var message = change.Value?.Messages?.FirstOrDefault();
                if (status is not null && status.Status == "sent")
                {
                    // É UMA NOTIFICAÇÃO SOBRE UMA MENSAGEM QUE NÓS ENVIAMOS!
                    // Precisamos buscar o texto da mensagem original, o que é complexo.
                    // UMA ABORDAGEM MAIS SIMPLES: A outra aplicação, ao enviar o template,
                    // pode chamar um endpoint simples no nosso CRM para registrar a mensagem.
                    // Vamos seguir com essa abordagem, pois é mais garantida.
                    }else if (message is not null)
                {
                    //var message = change.Value?.Messages?.FirstOrDefault(m => m.Type == "text");
                    var contactPayload = change.Value?.Contacts?.FirstOrDefault();

                    if (message is null || contactPayload is null) return Ok("Payload sem mensagem ou contato válido.");

                    var textoDaMensagem = message.Text.Body;
                    var telefoneDoContato = message.From;

                    if (_metaSettings.DeveloperPhoneNumbers.Any() && !_metaSettings.DeveloperPhoneNumbers.Contains(telefoneDoContato))
                    {
                        Console.WriteLine($"--> Mensagem do número {telefoneDoContato} ignorada (não está na lista de desenvolvedores).");
                        return Ok("Mensagem ignorada.");
                    }


                    var nomeDoContato = contactPayload.Profile.Name;

                    // 1. Verifica se existe uma sessão de bot ativa no Redis para este contato.
                    var botSession = await _botSessionCache.GetStateAsync(telefoneDoContato);

                    if (botSession is null)
                    {
                        // 2a. SE NÃO HÁ SESSÃO: Inicia um novo fluxo de autoatendimento.
                        // Esta lógica encontra ou cria o contato e depois chama o IniciarConversaHandler.

                        Guid contatoId;
                        var getContactQuery = new GetContactByTelefoneQuery(telefoneDoContato);
                        var contatoDto = await _getContactByTelefoneHandler.HandleAsync(getContactQuery);

                        if (contatoDto is null)
                        {
                            var createContactCommand = new CriarContatoCommand(nomeDoContato, telefoneDoContato);
                            var novoContatoDto = await _criarContatoHandler.HandleAsync(createContactCommand);
                            contatoId = novoContatoDto.Id;
                        }
                        else
                        {
                            contatoId = contatoDto.Id;
                        }


                        var iniciarConversaCommand = new IniciarConversaCommand(contatoId, textoDaMensagem);
                        await _iniciarConversaHandler.HandleAsync(iniciarConversaCommand);
                    }
                    else
                    {
                        // 2b. SE HÁ SESSÃO: Processa a resposta do usuário ao menu.
                        // Chama nosso novo handler para lidar com a lógica do bot.
                        var processarRespostaCommand = new ProcessarRespostaDoMenuCommand(telefoneDoContato, textoDaMensagem);
                        await _processarRespostaHandler.HandleAsync(processarRespostaCommand);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Erro ao processar webhook: {ex.Message} \n {ex.StackTrace}");
                return Ok(); // Sempre retorne 200 OK para a Meta
            }
        }

        private bool IsValidSignature(string payload, string signature)
        {
            // Lógica para validar o hash HMACSHA256 usando seu AppSecret
            // ...
            return true; // Implementação real da validação é necessária aqui
        }

    }


}

