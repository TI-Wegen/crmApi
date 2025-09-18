namespace Dashboard.Domain.Dtos;

public record DashboardPersonalResponseQuery(
    int ConversasResolvidas,
    int ConversasAtivas,
    int MediaAvaliacao,
    int ConversasPendentes,
    int ConversasEmAndamento
);