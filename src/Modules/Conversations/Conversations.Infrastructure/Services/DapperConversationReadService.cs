namespace Conversations.Infrastructure.Services;

using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Queries;
using Dapper;
// Em Modules/Conversations/Infrastructure/Services/ (pode criar esta pasta)
using System.Data;
using System.Text;

public class DapperConversationReadService : IConversationReadService
{
    private readonly IDbConnection _dbConnection;

    public DapperConversationReadService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    // A LÓGICA DO DAPPER FOI MOVIDA PARA CÁ
    public async Task<IEnumerable<ConversationSummaryDto>> GetAllSummariesAsync(
        GetAllConversationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append(@"
            SELECT
                c.""Id"",
                co.""Nome"" AS ContatoNome,
                co.""Telefone"" AS ContatoTelefone,
                ag.""Nome"" AS AgenteNome,
                c.""Status"",
                (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
                (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview
            FROM ""Conversas"" c
            INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
            LEFT JOIN ""Agentes"" ag ON c.""AgenteId"" = ag.""Id""
        ");

        var parameters = new DynamicParameters();
        var whereClauses = new List<string>();

        if (query.Status.HasValue)
        {
            whereClauses.Add(@"c.Status = @Status");
            parameters.Add("Status", query.Status.ToString());
        }
        if (query.AgenteId.HasValue)
        {
            whereClauses.Add(@"c.AgenteId = @AgenteId");
            parameters.Add("AgenteId", query.AgenteId.Value);
        }
        if (query.SetorId.HasValue)
        {
            whereClauses.Add(@"c.SetorId = @SetorId");
            parameters.Add("SetorId", query.SetorId.Value);
        }

        if (whereClauses.Any())
        {
            sqlBuilder.Append(" WHERE ");
            sqlBuilder.Append(string.Join(" AND ", whereClauses));
        }

        sqlBuilder.Append(" ORDER BY UltimaMensagemTimestamp DESC");
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.PageNumber - 1) * query.PageSize);

        return await _dbConnection.QueryAsync<ConversationSummaryDto>(
            new CommandDefinition(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken)
        );
    }

    public async Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        // A query é a mesma da listagem, mas filtrando por um ID específico.
        var sql = @"
        SELECT
            c.""Id"",
            COALESCE(co.""Nome"", 'Contato Removido') AS ContatoNome,
            COALESCE(co.""Telefone"", 'N/A') AS ContatoTelefone,
            ag.""Nome"" AS AgenteNome,
            c.""Status"",
            (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
            (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview
        FROM ""Conversas"" c
        LEFT JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
        LEFT JOIN ""Agentes"" ag ON c.""AgenteId"" = ag.""Id""
        WHERE c.""Id"" = @ConversationId
    ";

        return await _dbConnection.QueryFirstOrDefaultAsync<ConversationSummaryDto>(
            new CommandDefinition(sql, new { ConversationId = conversationId }, cancellationToken: cancellationToken)
        );
    }
}