using Agents.Application.Dtos;
using Agents.Application.UseCases.Commands;
using Agents.Application.UseCases.Queries;
using Conversations.Domain.Exceptions;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AgentsController : ControllerBase
{
    private readonly ICommandHandler<CriarAgenteCommand, AgenteDto> _criarAgenteHandler;
    private readonly IQueryHandler<GetAgentByIdQuery, AgenteDto> _getAgentByIdHandler;
    private readonly IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>> _getAllAgentsHandler;
    private readonly ICommandHandler<AtualizarAgenteCommand> _atualizarAgenteHandler; 

    public AgentsController(ICommandHandler<CriarAgenteCommand, AgenteDto> criarAgenteHandler, 
        IQueryHandler<GetAgentByIdQuery, AgenteDto> getAgentByIdHandler,
        IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>> getAllAgentsHandler,
        ICommandHandler<AtualizarAgenteCommand> atualizarAgenteHandler)
    {
        _criarAgenteHandler = criarAgenteHandler;
        _getAgentByIdHandler = getAgentByIdHandler;
        _getAllAgentsHandler = getAllAgentsHandler;
        _atualizarAgenteHandler = atualizarAgenteHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AgenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarAgenteCommand command)
    {
        try
        {
            var agenteDto = await _criarAgenteHandler.HandleAsync(command);
            // Retorna 201 Created com a localização do novo recurso e o próprio recurso no corpo.
            return CreatedAtAction(nameof(GetById), new { id = agenteDto.Id }, agenteDto);
        }
        catch (Exception ex) // Idealmente, uma exceção de aplicação mais específica
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}", Name = "GetAgentById")]
    [ProducesResponseType(typeof(AgenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetAgentByIdQuery(id);
            var agenteDto = await _getAgentByIdHandler.HandleAsync(query);
            return Ok(agenteDto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AgenteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAllAgentsQuery(pageNumber, pageSize);
        var agentes = await _getAllAgentsHandler.HandleAsync(query);
        return Ok(agentes);
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarAgenteRequest request)
    {
        try
        {
            var command = new AtualizarAgenteCommand(id, request.Nome, request.SetorIds);
            await _atualizarAgenteHandler.HandleAsync(command);

            // 204 NoContent é a resposta padrão para uma atualização bem-sucedida.
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
}
