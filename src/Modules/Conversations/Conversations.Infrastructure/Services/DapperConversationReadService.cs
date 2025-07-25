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
        SELECT DISTINCT ON (c.""Id"")
                    a.""Id"" AS AtendimentoId,
                    c.""Id"" AS Id,
                    co.""Nome"" AS ContatoNome,
                    co.""Telefone"" AS ContatoTelefone,
                    ag.""Nome"" AS AgenteNome,
                    a.""Status"",
                CASE 
                WHEN c.""SessaoFim"" > NOW() AT TIME ZONE 'UTC' THEN true 
                ELSE false 
                END AS SessaoWhatsappAtiva,
                c.""SessaoFim"" AS SessaoWhatsappExpiraEm,
                    -- Lógica da última mensagem permanece a mesma
                    (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
                    (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview
                FROM ""Atendimentos"" a
                INNER JOIN ""Conversas"" c ON a.""ConversaId"" = c.""Id""
                INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
                LEFT JOIN ""Agentes"" ag ON a.""AgenteId"" = ag.""Id""
        ");

        var parameters = new DynamicParameters();
        var whereClauses = new List<string>();

        var activeStatuses = new[] { "EmAutoAtendimento", "AguardandoNaFila", "EmAtendimento", "Resolvida", "FechadoSemResposta" , "AguardandoRespostaCliente" };

        whereClauses.Add(@"a.""Status"" = ANY(@ActiveStatuses)");
        parameters.Add("ActiveStatuses", activeStatuses);

        if (query.Status.HasValue)
        {
            whereClauses.Add(@"a.""Status"" = @Status");
            parameters.Add("Status", query.Status.ToString());
        }
        if (query.AgenteId.HasValue)
        {
            whereClauses.Add(@"a.""AgenteId"" = @AgenteId");
            parameters.Add("AgenteId", query.AgenteId.Value);
        }
        if (query.SetorId.HasValue)
        {
            whereClauses.Add(@"a.""SetorId"" = @SetorId");
            parameters.Add("SetorId", query.SetorId.Value);
        }

        if (whereClauses.Any())
        {
            sqlBuilder.Append(" WHERE ");
            sqlBuilder.Append(string.Join(" AND ", whereClauses));
        }

        sqlBuilder.Append(" ORDER BY c.\"Id\", UltimaMensagemTimestamp DESC");
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.PageNumber - 1) * query.PageSize);

        return await _dbConnection.QueryAsync<ConversationSummaryDto>(
            new CommandDefinition(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken)
        );
    }

    public async Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT
                a.""Id"" AS AtendimentoId,
                c.""Id"" AS Id,
                co.""Nome"" AS ContatoNome,
                co.""Telefone"" AS ContatoTelefone,
                ag.""Nome"" AS AgenteNome,
                a.""Status"",
                (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
                (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview
            FROM ""Conversas"" c
            INNER JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId""
            INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
            LEFT JOIN ""Agentes"" ag ON a.""AgenteId"" = ag.""Id""
            WHERE c.""Id"" = @ConversationId
            ORDER BY a.""CreatedAt"" DESC -- Supondo que você tenha uma coluna 'CreatedAt'
            LIMIT 1;
    ";

        return await _dbConnection.QueryFirstOrDefaultAsync<ConversationSummaryDto>(
            new CommandDefinition(sql, new { ConversationId = conversationId }, cancellationToken: cancellationToken)
        );
    }
}