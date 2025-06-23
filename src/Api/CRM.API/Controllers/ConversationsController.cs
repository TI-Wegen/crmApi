using Conversations.Application.Dtos;
using Conversations.Application.Exceptions;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Queries;
using Conversations.Domain.Enuns;
using Conversations.Domain.Exceptions;
using CRM.API.Dtos;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationsController : ControllerBase
{
    private readonly ICommandHandler<AtribuirAgenteCommand> _atribuirAgenteHandler;
    private readonly IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto> _getByIdHandler;
    private readonly ICommandHandler<IniciarConversaCommand, Guid> _iniciarConversaHandler;
    private readonly ICommandHandler<AdicionarMensagemCommand, MessageDto> _adicionarMensagemHandler;
    public ConversationsController(
 ICommandHandler<AtribuirAgenteCommand> atribuirAgenteHandler,
 IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto> getByIdHandler,
 ICommandHandler<IniciarConversaCommand, Guid> iniciarConversaHandler,
 ICommandHandler<AdicionarMensagemCommand, MessageDto> adicionarMensagemHandler)
    {
        _atribuirAgenteHandler = atribuirAgenteHandler;
        _getByIdHandler = getByIdHandler;
        _iniciarConversaHandler = iniciarConversaHandler;
        _adicionarMensagemHandler = adicionarMensagemHandler; // NOVO

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
            var command = new IniciarConversaCommand(request.ContatoId, request.Texto, request.AnexoUrl);
            var novaConversaId = await _iniciarConversaHandler.HandleAsync(command);

            // Retorna 201 Created com o header 'Location' apontando para o novo recurso
            // e o ID da nova conversa no corpo da resposta.
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
    public async Task<IActionResult> AdicionarMensagem(Guid id, [FromBody] AdicionarMensagemRequest request)
    {
        // É uma boa prática validar a string 'RemetenteTipo'
        if (!Enum.TryParse<RemetenteTipo>(request.RemetenteTipo, true, out var remetenteTipo))
        {
            return BadRequest("RemetenteTipo inválido. Use 'Agente' ou 'Cliente'.");
        }

        try
        {
            var command = new AdicionarMensagemCommand(id, request.Texto, request.AnexoUrl, remetenteTipo, request.AgenteId);
            var messageDto = await _adicionarMensagemHandler.HandleAsync(command);

            // Retorna 200 OK com os dados da mensagem recém-criada no corpo.
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
}
