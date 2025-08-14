using CRM.Application.ValueObject;

namespace CRM.Application.Interfaces;

public interface IMetaMessageSender
{
    Task EnviarMensagemTextoAsync(string numeroDestino, string textoMensagem);
    Task EnviarDocumentoAsync(string numeroDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda);
    Task EnviarImagemAsync(string numeroDestino, string urlDaImagem, string? legenda);
    Task EnviarAudioAsync(string numeroDestino, string urlDoAudio);
    Task<string> EnviarTemplateAsync(SendTemplateInput input);
    Task EnviarPesquisaDeSatisfacaoAsync(string numeroDestino, Guid atendimentoId);

}
