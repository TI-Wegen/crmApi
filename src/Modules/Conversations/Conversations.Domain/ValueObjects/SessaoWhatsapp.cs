namespace Conversations.Domain.ValueObjects;

public record SessaoWhatsapp
{
    public DateTime DataInicio { get; private init; }
    public DateTime? DataFim { get; private init; }

    private SessaoWhatsapp(DateTime dataInicio)
    {
        DataInicio = dataInicio;
        DataFim = null;
    }

    public static SessaoWhatsapp Iniciar(DateTime dataMensagem)
    {
        return new SessaoWhatsapp(dataMensagem);
    }

    public bool EstaAtiva()
    {
        return DataFim != null;
    }
}