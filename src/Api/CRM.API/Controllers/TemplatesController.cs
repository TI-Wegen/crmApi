using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Templates.Application.Dtos;
using Templates.Application.UseCases.Commands;
using Templates.Application.UseCases.Queries;

namespace CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ICommandHandler<CriarTemplateCommand, TemplateDto> _criarTemplateHandler;
        private readonly IQueryHandler<GetAllTemplatesQuery, IEnumerable<TemplateDto>> _getAllTemplatesHandler;

        public TemplatesController(ICommandHandler<CriarTemplateCommand, TemplateDto> criarTemplateHandler,
            IQueryHandler<GetAllTemplatesQuery, IEnumerable<TemplateDto>> getAllTemplatesHandler)
        {
            _criarTemplateHandler = criarTemplateHandler;
            _getAllTemplatesHandler = getAllTemplatesHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarTemplateCommand command)
        {
            var templateDto = await _criarTemplateHandler.HandleAsync(command);
            return CreatedAtAction(nameof(Criar), new { id = templateDto.Id }, templateDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var templates = await _getAllTemplatesHandler.HandleAsync(new GetAllTemplatesQuery());
            return Ok(templates);
        }
    }
}
