using System.Data;
using System.Text;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
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
                (SELECT m.""Timestamp"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemTimestamp,
                (SELECT m.""Texto"" FROM ""Mensagens"" m WHERE m.""ConversaId"" = c.""Id"" ORDER BY m.""Timestamp"" DESC LIMIT 1) AS UltimaMensagemPreview,
            T.""Nome"" AS TagName,
            T.""Id"" AS TagId,
            T.""Cor"" AS TagColor
            FROM ""Atendimentos"" a
            LEFT JOIN ""Tags"" T on T.""Id"" = a.""TagsId""
            INNER JOIN ""Conversas"" c ON a.""ConversaId"" = c.""Id""
            INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id""
            LEFT JOIN ""Agentes"" ag ON a.""AgenteId"" = ag.""Id""
    ");

    var parameters = new DynamicParameters();
    var whereClauses = new List<string>();

    var activeStatuses = new[]
    {
        "EmAutoAtendimento", "AguardandoNaFila", "EmAtendimento", "Resolvida", "FechadoSemResposta",
        "AguardandoRespostaCliente"
    };

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
    
    if (query.TagId.HasValue)
    {
        whereClauses.Add(@"a.""TagsId"" = @TagId");
        parameters.Add("TagId", query.TagId.Value);
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

    sqlBuilder.Append(" ORDER BY c.\"Id\", (SELECT m.\"Timestamp\" FROM \"Mensagens\" m WHERE m.\"ConversaId\" = c.\"Id\" ORDER BY m.\"Timestamp\" DESC LIMIT 1) DESC");
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
        CancellationToken cancellationToken)
    {
        var sql = @"
        -- Busca os detalhes da Conversa, Contato e do Atendimento Ativo
        SELECT
            c.""Id"", c.""ContatoId"",
            co.""Nome"" AS ContatoNome, -- NOVO: Buscando o nome do contato
            co.""Telefone"" AS ContatoTelefone, -- NOVO: Buscando o nome do contato
            a.""Id"" AS AtendimentoId, a.""AgenteId"", a.""SetorId"", a.""Status"", a.""BotStatus"",
          CASE 
                WHEN c.""SessaoFim"" > NOW() AT TIME ZONE 'UTC' THEN true 
                ELSE false 
                END AS SessaoWhatsappAtiva,
                c.""SessaoFim"" AS SessaoWhatsappExpiraEm
        FROM ""Conversas"" c
        INNER JOIN ""Contatos"" co ON c.""ContatoId"" = co.""Id"" -- Garantindo que temos o contato
        LEFT JOIN ""Atendimentos"" a ON c.""Id"" = a.""ConversaId"" 
            AND a.""Status"" IN ('EmAutoAtendimento', 'AguardandoNaFila', 'EmAtendimento', 'AguardandoRespostaCliente')
        WHERE c.""Id"" = @ConversationId;

        -- Busca TODAS as mensagens da conversa
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
        ORDER BY t.""Timestamp"" DESC;
    ";

        using var multi = await _dbConnection.QueryMultipleAsync(sql, new { ConversationId = conversationId });

        var details = await multi.ReadFirstOrDefaultAsync<ConversationDetailsDto>();
        if (details is null) return null;

        details.Mensagens = (await multi.ReadAsync<MessageDto>()).ToList();

        return details;
    }
}