using Conversations.Application.UseCases.Commands;
using CRM.API.Controllers.Base;
using CRM.API.Filters;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers
{
    [ApiKeyAuth]
    public class InternalApiController : BaseController
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
