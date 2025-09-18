using System.Data;
using System.Text;
using Dapper;
using Dashboard.Domain.Dtos;
using Dashboard.Domain.Repository;

namespace Dashboard.Infrastructure.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnection _dbConnection;

    public DashboardRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<DashboardFullResponseQuery?> GetFullDashboardAsync(CancellationToken cancellationToken = default)
    {
        var sql = @"
        SELECT
            (SELECT COUNT(*) FROM ""Mensagens"") AS ""TotalMensagens"",
            (SELECT COUNT(*) FROM ""Mensagens"" WHERE ""RemetenteTipo"" = 'Agente') AS ""TotalMensagemEnviadas"",
            (SELECT COUNT(*) FROM ""Mensagens"" WHERE ""RemetenteTipo"" = 'Cliente') AS ""TotalMensagemRecebidas"",
            (SELECT COUNT(*) FROM ""Mensagens"" WHERE ""RemetenteTipo"" = 'Cliente' AND DATE(""CreatedAt"") = CURRENT_DATE) AS ""TotalMensagemRecebidasHoje"",
            (SELECT COUNT(*) FROM ""Mensagens"" WHERE ""RemetenteTipo"" = 'Cliente' AND ""CreatedAt"" >= CURRENT_DATE - INTERVAL '7 days') AS ""TotalMensagemRecebidasSemana"",
            (SELECT COUNT(*) FROM ""Mensagens"" WHERE ""RemetenteTipo"" = 'Cliente' AND ""CreatedAt"" >= CURRENT_DATE - INTERVAL '30 days') AS ""TotalMensagemRecebidasMes"",
            (SELECT COUNT(*) FROM ""Atendimentos"") AS ""TotalAtendimentos"",
            (SELECT COUNT(*) FROM ""Atendimentos"" WHERE ""Status"" = 'AguardandoNaFila') AS ""TotalAguardandoNaFila"",
            (SELECT COUNT(*) FROM ""Atendimentos"" WHERE ""Status"" = 'EmAtendimento') AS ""TotalEmAtendimento"",
            (SELECT COUNT(*) FROM ""Atendimentos"" WHERE ""Status"" = 'Resolvida') AS ""TotalResolvidas"";";

        var result = await _dbConnection.QueryFirstOrDefaultAsync<DashboardFullResponseQuery>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return result;
    }

    public async Task<DashboardPersonalResponseQuery?> GetProfileDashboardAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
        SELECT
            COUNT(*) FILTER (WHERE a.""Status"" = 'Resolvida') AS ""ConversasResolvidas"",
            COUNT(*) FILTER (WHERE a.""Status"" != 'Resolvida') AS ""ConversasAtivas"",
            ROUND(AVG(a.""AvaliacaoNota"") FILTER (
                WHERE a.""Status"" = 'Resolvida'
                    AND a.""AvaliacaoNota"" IS NOT NULL
                ), 2) AS ""MediaAvaliacao"",
            COUNT(*) FILTER (WHERE a.""Status"" != 'AguardandoRespostaCliente') AS ""ConversasPendentes"",
            COUNT(*) FILTER (WHERE a.""Status"" != 'EmAtendimento') AS ""ConversasEmAndamento""

        FROM ""Agentes"" ag
                 LEFT JOIN ""Atendimentos"" a ON a.""AgenteId"" = ag.""Id""
        WHERE ag.""Id"" = @Agente;";

        var result = await _dbConnection.QueryFirstOrDefaultAsync<DashboardPersonalResponseQuery>(
            new CommandDefinition(sql, new { Agente = id }, cancellationToken: cancellationToken));

        return result;
    }
}