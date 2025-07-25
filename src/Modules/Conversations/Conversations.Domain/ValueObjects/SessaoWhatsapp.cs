namespace Conversations.Domain.ValueObjects;

public record SessaoWhatsapp
{
    public DateTime DataInicio { get; private init; }
    public DateTime DataFim { get; private init; }

    private SessaoWhatsapp(DateTime dataInicio)
    {
        DataInicio = dataInicio;
        DataFim = dataInicio.AddHours(24);
    }

    public static SessaoWhatsapp Iniciar(DateTime dataMensagem)
    {
        return new SessaoWhatsapp(dataMensagem);
    }

    public bool EstaAtiva(DateTime dataAtual)
    {
        return dataAtual < DataFim;
    }
}