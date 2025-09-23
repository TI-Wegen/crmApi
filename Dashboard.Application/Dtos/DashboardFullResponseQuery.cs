namespace Dashboard.Application.Dtos;

public record DashboardFullResponseQuery(
    long TotalMensagens,
    long TotalMensagemEnviadas,
    long TotalMensagemRecebidas,
    long TotalMensagemRecebidasHoje,
    long TotalMensagemRecebidasSemana,
    long TotalMensagemRecebidasMes,
    long TotalAtendimentos,
    long TotalAguardandoNaFila,
    long TotalEmAtendimento,
    long TotalResolvidas
);
