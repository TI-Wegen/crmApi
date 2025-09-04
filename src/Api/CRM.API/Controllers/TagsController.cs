using CRM.Application.Dto;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Tags.Application.Dtos;
using Tags.Application.UseCases.Commands;
using Tags.Application.UseCases.Queries;

namespace CRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly ICommandHandler<CriarTagCommand, TagDto> _criarTagHandler;
    private readonly ICommandHandler<AtualizarTagCommand, TagDto> _atualizarTagHandler;
    private readonly ICommandHandler<InativarTagCommand, TagDto> _deletarTagHandler;
    private readonly IQueryHandler<GetAllTagsQuery, PaginationDto<TagDto>> _getAllTagHandler;
    private readonly IQueryHandler<GetTagByIdQuery, TagDto> _getbyIdTagHandler;

    public TagsController(
        ICommandHandler<CriarTagCommand, TagDto> criarTagHandler,
        ICommandHandler<AtualizarTagCommand, TagDto> atualizarTagHandler,
        ICommandHandler<InativarTagCommand, TagDto> deletarTagHandler,
        IQueryHandler<GetAllTagsQuery, PaginationDto<TagDto>> getAllTagHandler,
        IQueryHandler<GetTagByIdQuery, TagDto> getbyIdTagHandler
    )
    {
        _criarTagHandler = criarTagHandler;
        _atualizarTagHandler = atualizarTagHandler;
        _deletarTagHandler = deletarTagHandler;
        _getAllTagHandler = getAllTagHandler;
        _getbyIdTagHandler = getbyIdTagHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarTagCommand command)
    {
        try
        {
            var tagDto = await _criarTagHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = tagDto.Id }, tagDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}", Name = "GetTagsById")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetTagByIdQuery(id);
            var contatoDto = await _getbyIdTagHandler.HandleAsync(query);
            return Ok(contatoDto);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginationDto<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetAllTagsQuery(pageNumber, pageSize);
        var result = await _getAllTagHandler.HandleAsync(query);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTagCommand request)
    {
        try
        {
            var command = request with { Id = id };
            var response = await _atualizarTagHandler.HandleAsync(command);

            return Ok(response);
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
            var command = new InativarTagCommand(id);
            await _deletarTagHandler.HandleAsync(command);

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