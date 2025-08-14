using CRM.Application.ValueObject;

namespace CRM.Application.Interfaces;

public interface IMensageriaBotService
{
    Task EnviarEMensagemTextoAsync(Guid atendimentoId, string telefoneDestino, string texto);
    Task EnviarEDocumentoAsync(Guid atendimentoId, string telefoneDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda);
    Task<string?> EnviarETemplateAsync(Guid atendimentoId, SendTemplateInput sendTemplateInput);


}


