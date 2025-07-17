using Metrics.Application.abstractions;
using Dapper;
using System.Data;
using Metrics.Application.Dtos;
using Metrics.Domain.Entities;

namespace Metrics.Infrastructure.services;

public class DapperTemplateMetricsService : ITemplateMetricsReadService
{
    private readonly IDbConnection _dbConnection;
    public DapperTemplateMetricsService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task AddTemplateSentMetricAsync(MetricaTemplateEnviado metricaTemplateEnviado)
    {
        const string sql = @"
            INSERT INTO ""MetricasTemplatesEnviados"" (""Id"", ""AtendimentoId"", ""AgenteId"", ""TemplateName"", ""SentAt"")
            VALUES (@Id, @AtendimentoId, @AgenteId, @TemplateName, @SentAt);
        ";
        await _dbConnection.ExecuteAsync(sql, metricaTemplateEnviado);
    }

    public async Task<IEnumerable<TemplatesSentPerAgentDto>> GetSentCountPerAgentAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT
                m.""AgenteId"",
                a.""Nome"" AS AgenteNome,
                COUNT(m.""Id"") AS TotalEnviado
            FROM ""MetricasTemplatesEnviados"" m
            INNER JOIN ""Agentes"" a ON m.""AgenteId"" = a.""Id""
            WHERE m.""SentAt"" BETWEEN @StartDate AND @EndDate
            GROUP BY m.""AgenteId"", a.""Nome""
            ORDER BY TotalEnviado DESC;
        ";

        return await _dbConnection.QueryAsync<TemplatesSentPerAgentDto>(sql, new { StartDate = startDate, EndDate = endDate });
    }
}

