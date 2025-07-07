namespace Conversations.Application.Abstractions
{
    public interface IMetaMessageSender
    {
        Task EnviarMensagemTextoAsync(string numeroDestino, string textoMensagem);
        Task EnviarDocumentoAsync(string numeroDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda);
        Task EnviarImagemAsync(string numeroDestino, string urlDaImagem, string? legenda);
        Task EnviarAudioAsync(string numeroDestino, string urlDoAudio);


    }

}
