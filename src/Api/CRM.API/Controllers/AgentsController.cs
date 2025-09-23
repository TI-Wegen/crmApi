using Agents.Application.Dtos;
using Agents.Application.UseCases.Commands;
using Agents.Application.UseCases.Queries;
using Conversations.Application.UseCases.Commands;
using CRM.API.Controllers.Base;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using AtualizarAgenteCommand = Conversations.Application.UseCases.Commands.AtualizarAgenteCommand;

namespace CRM.API.Controllers;

public class AgentsController : BaseController
{
    private readonly ICommandHandler<CriarAgenteCommand, AgenteDto> _criarAgenteHandler;
    private readonly ICommandHandler<Agents.Application.UseCases.Commands.AtualizarAgenteCommand> _atualizarAgenteHandler;
    private readonly ICommandHandler<InativarAgenteCommand> _inativarAgenteHandler;
    private readonly IQueryHandler<GetAgentByIdQuery, AgenteDto> _getAgentByIdHandler;
    private readonly IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>> _getAllAgentsHandler;
    private readonly IQueryHandler<GetSetoresQuery, IEnumerable<SetorDto>> _getSetoresHandler;


    public AgentsController(ICommandHandler<CriarAgenteCommand, AgenteDto> criarAgenteHandler,
        IQueryHandler<GetAgentByIdQuery, AgenteDto> getAgentByIdHandler,
        IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>> getAllAgentsHandler,
        ICommandHandler<Agents.Application.UseCases.Commands.AtualizarAgenteCommand> atualizarAgenteHandler,
        ICommandHandler<InativarAgenteCommand> inativarAgenteHandler,
        IQueryHandler<GetSetoresQuery, IEnumerable<SetorDto>> getSetoresHandler
        )
    {
        _criarAgenteHandler = criarAgenteHandler;
        _getAgentByIdHandler = getAgentByIdHandler;
        _getAllAgentsHandler = getAllAgentsHandler;
        _atualizarAgenteHandler = atualizarAgenteHandler;
        _inativarAgenteHandler = inativarAgenteHandler;
        _getSetoresHandler = getSetoresHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AgenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarAgenteCommand command)
    {
        try
        {
            var agenteDto = await _criarAgenteHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = agenteDto.Id }, agenteDto);
        }
        catch (Exception ex) 
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
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarAgenteCommand request)
    {
        try
        {
            var command = new Agents.Application.UseCases.Commands.AtualizarAgenteCommand(id, request.Nome, request.SetorIds);
            await _atualizarAgenteHandler.HandleAsync(command);
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


    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new InativarAgenteCommand(id);
            await _inativarAgenteHandler.HandleAsync(command);
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


    [HttpGet("setores")]
    [ProducesResponseType(typeof(IEnumerable<SetorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSetores()
    {
        var query = new GetSetoresQuery();
        var setores = await _getSetoresHandler.HandleAsync(query );
        return Ok(setores);
    }
}