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


    public async Task<DashboardPersonalResponseQuery?> GetProfileDashboardAsync(Guid id, CancellationToken cancellationToken = default)
    {
        StringBuilder sql = new StringBuilder();
        sql.Append(@"
        select (select count(*) from ""Atendimentos"" where ""Status"" = 'Resolvida' and ""AgenteId"" = @Agente)               as ""ConversasResolvidas"",
               (select count(*) from ""Conversas"" where ""Status"" != 'Resolvida' and ""AgenteId"" = @Agente)                 as ""ConversasAtivas"",
               (select round(avg(""AvaliacaoNota""), 2), ""AgenteId""
                from ""Atendimentos""
                where ""Status"" = 'Resolvida' and ""AgenteId"" = @Agente
                  and ""AvaliacaoNota"" is not null
                group by ""AgenteId"")                                                            as ""MediaAvaliacao"",
               (select count(*) from ""Conversas"" where ""Status"" != 'AguardandoRespostaCliente') as ""ConversasPendentes"",
               (select count(*) from ""Conversas"" where ""Status"" != 'EmAtendimento')             as ""ConversasEmAndamento""
        from ""Atendimentos"";
        ");
        
        var parameters = new DynamicParameters();
        var whereClauses = new List<string>();
        
        var result = await _dbConnection.QueryFirstOrDefaultAsync<DashboardPersonalResponseQuery>(
            new CommandDefinition(sql.ToString(), new {Agente = id},cancellationToken: cancellationToken));

        return result;
    }
}