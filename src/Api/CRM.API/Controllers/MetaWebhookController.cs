using Amazon.Runtime.Internal;
using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Queries;
using Conversations.Application.UseCases.Commands;
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
        public MetaWebhookController(
            IOptions<MetaSettings> metaSettings,
            ICommandHandler<CriarContatoCommand, ContatoDto> criarContatoHandler,
            ICommandHandler<IniciarConversaCommand, Guid> iniciarConversaHandler,
            IQueryHandler<GetContactByTelefoneQuery, ContatoDto?> getContactByTelefoneHandler)

        {
            _metaSettings = metaSettings.Value;
            _criarContatoHandler = criarContatoHandler;
            _iniciarConversaHandler = iniciarConversaHandler;
            _getContactByTelefoneHandler = getContactByTelefoneHandler;
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
            try
            {
                // A Meta pode enviar múltiplas entradas ou alterações em um único post.
                foreach (var change in payload.Entry.SelectMany(e => e.Changes))
                {
                    // Ignora alterações que não são do tipo "messages"
                    if (change.Field != "messages") continue;

                    // Processa apenas as mensagens de texto de usuários
                    var message = change.Value?.Messages?.FirstOrDefault(m => m.Type == "text");
                    var contactPayload = change.Value?.Contacts?.FirstOrDefault();

                    if (message is null || contactPayload is null) continue;

                    var textoDaMensagem = message.Text.Body;
                    var telefoneDoContato = message.From;
                    var nomeDoContato = contactPayload.Profile.Name;

                    // --- ORQUESTRAÇÃO DOS CASOS DE USO ---
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

                    var iniciarConversaCommand = new IniciarConversaCommand(contatoId, textoDaMensagem, null, null, null);
                    await _iniciarConversaHandler.HandleAsync(iniciarConversaCommand);
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

