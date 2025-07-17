using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Queries;
using Conversations.Domain.Enuns;
using CRM.API.Dtos;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using CRM.Infrastructure.Config.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;
//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ConversationsController : ControllerBase
{
    private readonly ICommandHandler<AtribuirAgenteCommand> _atribuirAgenteHandler;
    private readonly IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto> _getByIdHandler;
    private readonly ICommandHandler<IniciarConversaCommand, Guid> _iniciarConversaHandler;
    private readonly ICommandHandler<AdicionarMensagemCommand, MessageDto> _adicionarMensagemHandler;
    private readonly ICommandHandler<ResolverAtendimentoCommand> _resolverAtendimentoHandler;
    private readonly ICommandHandler<TransferirAtendimentoCommand> _transferirAtendimentoHandler;
    private readonly IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>> _getAllConversationsHandler;
    private readonly IQueryHandler<GetActiveChatQuery, ActiveChatDto> _getActiveChatHandler;
    private readonly ICommandHandler<EnviarTemplateCommand> _enviarTemplateHandler; 

    public ConversationsController(ICommandHandler<AtribuirAgenteCommand> atribuirAgenteHandler,
        IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto> getByIdHandler,
        ICommandHandler<IniciarConversaCommand, Guid> iniciarConversaHandler,
        ICommandHandler<AdicionarMensagemCommand, MessageDto> adicionarMensagemHandler,
        ICommandHandler<ResolverAtendimentoCommand> resolverAtendimentoHandler,
        ICommandHandler<TransferirAtendimentoCommand> transferirAtendimentoHandler,
        IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>> getAllConversationsHandler,
        IQueryHandler<GetActiveChatQuery, ActiveChatDto> getActiveChatHandler,
        ICommandHandler<EnviarTemplateCommand> enviarTemplateHandler
        )
    {
        _atribuirAgenteHandler = atribuirAgenteHandler;
        _getByIdHandler = getByIdHandler;
        _iniciarConversaHandler = iniciarConversaHandler;
        _adicionarMensagemHandler = adicionarMensagemHandler;
        _resolverAtendimentoHandler = resolverAtendimentoHandler;
        _transferirAtendimentoHandler = transferirAtendimentoHandler;
        _getAllConversationsHandler = getAllConversationsHandler;
        _getActiveChatHandler = getActiveChatHandler;
        _enviarTemplateHandler = enviarTemplateHandler;
    }

    [HttpGet("{id:guid}", Name = "GetConversationById")]
    [ProducesResponseType(typeof(ConversationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetConversationByIdQuery(id);
            var conversation = await _getByIdHandler.HandleAsync(query);
            return Ok(conversation);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/atribuir-agente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtribuirAgente(Guid id, [FromBody] AtribuirAgenteRequest request)
    {
        try
        {
            // Mapeamos o DTO da requisição para o nosso Comando interno.
            var command = new AtribuirAgenteCommand(id, request.AgenteId);
            await _atribuirAgenteHandler.HandleAsync(command);
            return NoContent(); // 204: Sucesso, sem conteúdo para retornar.
        }
        catch (NotFoundException ex)
        {
            // Ocorre se a conversa não for encontrada
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            // Ocorre se uma regra de negócio for violada (ex: atribuir conversa já resolvida)
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Iniciar([FromBody] IniciarConversaRequest request)
    {
        try
        {
            var command = new IniciarConversaCommand(request.ContatoId,
                request.Texto,
                  request.Anexo?.OpenReadStream(),
                  request.Anexo?.FileName,
                  request.Anexo?.ContentType);

            var novaConversaId = await _iniciarConversaHandler.HandleAsync(command);

            return CreatedAtAction(nameof(GetById), new { id = novaConversaId }, new { id = novaConversaId });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdicionarMensagem(
    [FromRoute] Guid id,
    [FromForm] AdicionarMensagemRequest request)
    {
        if (!Enum.TryParse<RemetenteTipo>(request.RemetenteTipo, true, out var remetenteTipo))
        {
            return BadRequest("RemetenteTipo inválido. Use 'Agente' ou 'Cliente'.");
        }

        try
        {
            var command = new AdicionarMensagemCommand(
                id,
                request.Texto,
                request.AnexoUrl,
                remetenteTipo,
                request.Anexo?.OpenReadStream(),
                request.Anexo?.FileName,
                request.Anexo?.ContentType
            );

            var messageDto = await _adicionarMensagemHandler.HandleAsync(command);

            return Ok(messageDto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPatch("{id:guid}/resolver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Resolver(Guid id)
    {
        try
        {
            var command = new ResolverAtendimentoCommand(id);
            await _resolverAtendimentoHandler.HandleAsync(command);

            // 204 NoContent é a resposta ideal para um comando de sucesso que não retorna dados.
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            // Se tentarmos resolver uma conversa que não está "EmAtendimento", cairá aqui.
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/transferir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transferir(Guid id, [FromBody] TransferirConversaRequest request)
    {
        try
        {
            var command = new TransferirAtendimentoCommand(id, request.NovoAgenteId, request.NovoSetorId);
            await _transferirAtendimentoHandler.HandleAsync(command);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConversationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllConversationsQuery query)
    {
        var result = await _getAllConversationsHandler.HandleAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}/active-chat", Name = "GetActiveChat")]
    public async Task<IActionResult> GetActiveChat(Guid id)
    {
        var query = new GetActiveChatQuery(id);
        var chatData = await _getActiveChatHandler.HandleAsync(query); // Injete o novo handler
        return Ok(chatData);
    }

    [HttpPost("{id:guid}/send-template")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendTemplate(Guid id, [FromBody] SendTemplateRequest request)
    {
        var command = new EnviarTemplateCommand(id, request.TemplateName, request.BodyParameters);
        await _enviarTemplateHandler.HandleAsync(command);

        return Accepted();
    }


}
