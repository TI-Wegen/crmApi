using Conversations.Application.UseCases.Commands;
using CRM.API.Filters;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers
{
    [ApiKeyAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class InternalApiController : ControllerBase
    {
        private readonly ICommandHandler<RegistrarMensagemEnviadaCommand> _handler;

        public InternalApiController(ICommandHandler<RegistrarMensagemEnviadaCommand> handler)
        {
            _handler = handler;
        }

        [HttpPost("sent")]
        public async Task<IActionResult> RegisterSentMessage([FromBody] RegistrarMensagemEnviadaCommand command)
        {
            await _handler.HandleAsync(command);
            return Ok();
        }
    }
}
