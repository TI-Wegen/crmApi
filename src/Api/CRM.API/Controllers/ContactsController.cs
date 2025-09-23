using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Queries;
using Conversations.Application.UseCases.Commands;
using CRM.API.Controllers.Base;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using CRM.Infrastructure.Config.Meta;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

public class ContactsController : BaseController
{
    private readonly ICommandHandler<CriarContatoCommand, ContatoDto> _criarContatoHandler;
    private readonly IQueryHandler<GetContactByIdQuery, ContatoDto> _getContactByIdHandler;
    private readonly IQueryHandler<GetAllContactsQuery, IEnumerable<ContatoDto>> _getAllContactsHandler;
    private readonly ICommandHandler<AtualizarContatoCommand> _atualizarContatoHandler;
    private readonly ICommandHandler<InativarContatoCommand> _inativarContatoHandler;
    private readonly ICommandHandler<EnviarTemplateCommand> _enviarTemplateHandler;
    public ContactsController(
        ICommandHandler<CriarContatoCommand, ContatoDto> criarContatoHandler,
        IQueryHandler<GetContactByIdQuery, ContatoDto> getContactByIdHandler,
        IQueryHandler<GetAllContactsQuery, IEnumerable<ContatoDto>> getAllContactsHandler,
        ICommandHandler<AtualizarContatoCommand> atualizarContatoHandler,
        ICommandHandler<InativarContatoCommand> inativarContatoHandler,
        ICommandHandler<EnviarTemplateCommand> enviarTemplateHandler
    )
    {
        _criarContatoHandler = criarContatoHandler;
        _getContactByIdHandler = getContactByIdHandler;
        _getAllContactsHandler = getAllContactsHandler;
        _atualizarContatoHandler = atualizarContatoHandler;
        _inativarContatoHandler = inativarContatoHandler;
        _enviarTemplateHandler = enviarTemplateHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContatoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarContatoCommand command)
    {
        try
        {
            var contatoDto = await _criarContatoHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = contatoDto.Id }, contatoDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}", Name = "GetContactById")]
    [ProducesResponseType(typeof(ContatoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetContactByIdQuery(id);
            var contatoDto = await _getContactByIdHandler.HandleAsync(query);
            return Ok(contatoDto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContatoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetAllContactsQuery(pageNumber, pageSize);
        var contatos = await _getAllContactsHandler.HandleAsync(query);
        return Ok(contatos);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContatoRequest request)
    {
        try
        {
            var command = new AtualizarContatoCommand(id, request.Nome, request.Telefone, request.Tags);
            await _atualizarContatoHandler.HandleAsync(command);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
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
            var command = new InativarContatoCommand(id);
            await _inativarContatoHandler.HandleAsync(command);

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

    [HttpPost("{id:guid}/send-template")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> SendTemplate(Guid id, [FromBody] SendTemplateRequest request)
    {
        var command = new EnviarTemplateCommand(id, request.TemplateName, request.BodyParameters);
        await _enviarTemplateHandler.HandleAsync(command);

        return Accepted();
    }
}