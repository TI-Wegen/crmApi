using Microsoft.AspNetCore.Http;

namespace Conversations.Application.UseCases.Commands;

public record IniciarConversaRequest(Guid ContatoId, string Texto, string ContatoNome, IFormFile? Anexo);