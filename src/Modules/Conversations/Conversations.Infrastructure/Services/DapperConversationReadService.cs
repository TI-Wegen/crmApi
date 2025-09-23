using System.Data;
using System.Text;
using Conversations.Application.Dtos;
using Conversations.Application.Services;
using Conversations.Application.UseCases.Queries;
using Dapper;

namespace Conversations.Infrastructure.Services;

public class DapperConversationReadService : IConversationReadService
{
    private readonly IDbConnection _dbConnection;

    public DapperConversationReadService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IEnumerable<ConversationSummaryDto>> GetAllSummariesAsync(
        GetAllConversationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append(@"
        SELECT
            a_last.""Id""                      AS AtendimentoId,
            c.""Id""                           AS Id,
            co.""Nome""                        AS ContatoNome,
            co.""Telefone""                    AS ContatoTelefone,
            ag.""Nome""                        AS AgenteNome,
            a_last.""Status""                  AS Status,
            CASE WHEN c.""SessaoFim"" > NOW() AT TIME ZONE 'UTC' THEN true ELSE false END
                                             AS SessaoWhatsappAtiva,
            c.""SessaoFim""                    AS SessaoWhatsappExpiraEm,
            m_last.""Timestamp""               AS UltimaMensagemTimestamp,
            m_last.""Texto""                   AS UltimaMensagemPreview,
            t.""Nome""                         AS TagName,
            t.""Id""                           AS TagId,
            t.""Cor""                          AS TagColor
        FROM ""Conversas"" c
                 LEFT JOIN LATERAL (
            SELECT a2.*
            FROM ""Atendimentos"" a2
            WHERE a2.""ConversaId"" = c.""Id""
            ORDER BY a2.""CreatedAt"" DESC
            LIMIT 1
            ) a_last ON true
                 LEFT JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
                 LEFT JOIN ""Agentes"" ag ON a_last.""AgenteId"" = ag.""Id""
                 LEFT JOIN ""Tags"" t ON t.""Id"" = c.""TagsId""
                 LEFT JOIN LATERAL (
            SELECT m.""Timestamp"", m.""Texto""
            FROM ""Mensagens"" m
            WHERE m.""ConversaId"" = c.""Id""
            ORDER BY m.""Timestamp"" DESC
            LIMIT 1
            ) m_last ON true
    ");

        var parameters = new DynamicParameters();
        var whereClauses = new List<string>();
        
        if (query.TagId.HasValue)
        {
            whereClauses.Add(@"t.""Id"" = @TagId");
            parameters.Add("TagId", query.TagId.Value);
        }
        
        if (whereClauses.Any())
        {
            sqlBuilder.Append(" WHERE ");
            sqlBuilder.Append(string.Join(" AND ", whereClauses));
        }

        sqlBuilder.Append(
            " ORDER BY m_last.\"Timestamp\" DESC NULLS LAST");
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.PageNumber - 1) * query.PageSize);

        return await _dbConnection.QueryAsync<ConversationSummaryDto>(
            new CommandDefinition(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken)
        );
    }

    public async Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
        SELECT
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
            (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
            (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview
        FROM ""Conversas"" c
        INNER JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId""
        INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
        LEFT JOIN ""Agentes"" ag ON a.""AgenteId"" = ag.""Id""
        WHERE c.""Id"" = @ConversationId
        ORDER BY a.""CreatedAt"" DESC 
        LIMIT 1;
    ";

        return await _dbConnection.QueryFirstOrDefaultAsync<ConversationSummaryDto>(
            new CommandDefinition(sql, new { ConversationId = conversationId }, cancellationToken: cancellationToken)
        );
    }

    public async Task<ConversationDetailsDto?> GetConversationDetailsAsync(Guid conversationId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var countSql = @"
        SELECT COUNT(*) 
        FROM (
            SELECT 1
            FROM ""Mensagens"" m
            WHERE m.""ConversaId"" = @ConversationId
            GROUP BY m.""Texto"", m.""ConversaId""
        ) t;
    ";

        var totalCount = await _dbConnection.QueryFirstOrDefaultAsync<int>(
            new CommandDefinition(countSql, new { ConversationId = conversationId },
                cancellationToken: cancellationToken));

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var sql = @"
        SELECT
            c.""Id"", c.""ContatoId"",
            co.""Nome"" AS ContatoNome,
            co.""Telefone"" AS ContatoTelefone,
            a.""Id"" AS AtendimentoId, a.""AgenteId"", a.""SetorId"", a.""Status"", a.""BotStatus"",
          CASE 
                WHEN c.""SessaoFim"" > NOW() AT TIME ZONE 'UTC' THEN true 
                ELSE false 
                END AS SessaoWhatsappAtiva,
                c.""SessaoFim"" AS SessaoWhatsappExpiraEm
        FROM ""Conversas"" c
        INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
        LEFT JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId"" 
            AND a.""Status"" IN ('EmAutoAtendimento', 'AguardandoNaFila', 'EmAtendimento', 'AguardandoRespostaCliente')
        WHERE c.""Id"" = @ConversationId;

        SELECT *
        FROM (
                 SELECT
                     m.*,
                     ROW_NUMBER() OVER (
                         PARTITION BY m.""Texto"", m.""ConversaId""
                         ORDER BY m.""Timestamp"" DESC
                         ) AS rn
                 FROM ""Mensagens"" m
                 WHERE m.""ConversaId"" = @ConversationId
             ) t
        WHERE t.rn = 1
        ORDER BY t.""Timestamp"" DESC
        LIMIT @PageSize OFFSET @Offset;
    ";

        var parameters = new
        {
            ConversationId = conversationId,
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        using var multi = await _dbConnection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var details = await multi.ReadFirstOrDefaultAsync<ConversationDetailsDto>();
        if (details is null) return null;

        details.Mensagens = (await multi.ReadAsync<MessageDto>()).ToList();

        details.CurrentPage = pageNumber;
        details.PageSize = pageSize;
        details.TotalCount = totalCount;
        details.TotalPages = totalPages;

        return details;
    }

    public async Task<ConversationDetailsDto?> GetConversationDetailsByContactAsync(Guid contactId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var conversationIdSql = @"
        SELECT c.""Id""
        FROM ""Conversas"" c
        INNER JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId"" 
            AND a.""Status"" IN ('EmAutoAtendimento', 'AguardandoNaFila', 'EmAtendimento', 'AguardandoRespostaCliente')
        WHERE c.""ContatoId"" = @ContactId
        ORDER BY c.""CreatedAt"" DESC
        LIMIT 1;
    ";

        var conversationId = await _dbConnection.QueryFirstOrDefaultAsync<Guid?>(
            new CommandDefinition(conversationIdSql, new { ContactId = contactId },
                cancellationToken: cancellationToken));

        if (!conversationId.HasValue)
            return null;

        var countSql = @"
        SELECT COUNT(*) 
        FROM (
            SELECT 1
            FROM ""Mensagens"" m
            WHERE m.""ConversaId"" = @ConversationId
            GROUP BY m.""Texto"", m.""ConversaId""
        ) t;
    ";

        var totalCount = await _dbConnection.QueryFirstOrDefaultAsync<int>(
            new CommandDefinition(countSql, new { ConversationId = conversationId.Value },
                cancellationToken: cancellationToken));

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var sql = @"
        SELECT
            c.""Id"", c.""ContatoId"",
            co.""Nome"" AS ContatoNome,
            co.""Telefone"" AS ContatoTelefone,
            a.""Id"" AS AtendimentoId, a.""AgenteId"", a.""SetorId"", a.""Status"", a.""BotStatus"",
          CASE 
                WHEN c.""SessaoFim"" > NOW() AT TIME ZONE 'UTC' THEN true 
                ELSE false 
                END AS SessaoWhatsappAtiva,
                c.""SessaoFim"" AS SessaoWhatsappExpiraEm
        FROM ""Conversas"" c
        INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
        LEFT JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId"" 
            AND a.""Status"" IN ('EmAutoAtendimento', 'AguardandoNaFila', 'EmAtendimento', 'AguardandoRespostaCliente')
        WHERE c.""Id"" = @ConversationId;

        SELECT *
        FROM (
                 SELECT
                     m.*,
                     ROW_NUMBER() OVER (
                         PARTITION BY m.""Texto"", m.""ConversaId""
                         ORDER BY m.""Timestamp"" DESC
                         ) AS rn
                 FROM ""Mensagens"" m
                 WHERE m.""ConversaId"" = @ConversationId
             ) t
        WHERE t.rn = 1
        ORDER BY t.""Timestamp"" DESC
        LIMIT @PageSize OFFSET @Offset;
    ";

        var parameters = new
        {
            ConversationId = conversationId.Value,
            PageSize = pageSize,
            Offset = (pageNumber - 1) * pageSize
        };

        using var multi = await _dbConnection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var details = await multi.ReadFirstOrDefaultAsync<ConversationDetailsDto>();
        if (details is null) return null;

        details.Mensagens = (await multi.ReadAsync<MessageDto>()).ToList();

        details.CurrentPage = pageNumber;
        details.PageSize = pageSize;
        details.TotalCount = totalCount;
        details.TotalPages = totalPages;

        return details;
    }
}