namespace Conversations.Application.Abstractions
{
    public interface IMetaMessageSender
    {
        Task EnviarMensagemTextoAsync(string numeroDestino, string textoMensagem);

    }

}
