namespace Dashboard.Domain.Dtos;

public record DashboardPersonalResponseQuery(
    long ConversasResolvidas,
    long ConversasAtivas,
    decimal MediaAvaliacao,
    long ConversasPendentes,
    long ConversasEmAndamento
);
