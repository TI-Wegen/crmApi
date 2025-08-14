using Boletos.Domain.Entities;
using Boletos.Domain.Enuns;
using Boletos.Domain.Repositories;
using Dapper;
using Infrastructure.ExternalServices.DataBase;


namespace Infrastructure.ExternalServices.Services.ClientService;

public class ClientRepository : IClientRepository
{
    private readonly IDbConnectionFactory _dbConnection;

    public ClientRepository(IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task CreateWpp(Client client)
    {
        string query = $@"INSERT INTO tblwhatsapp (idConta, numero, motivo, dataEnvio, jsonMensagem, mensagemErro)
VALUES (@idConta, @numero, @motivo, @dataEnvio, @jsonMensagem, @mensagemErro)";

        using var connection = _dbConnection.CreateConnection();
        var result = await connection.ExecuteAsync(query, new
        {
            idConta = client.IdConta,
            numero = client.Phone,
            motivo = client.Motive,
            dataEnvio = DateTime.Now,
            jsonMensagem = client.MessageId,
            jsonEnvioEconomia = client.EconomyUrl,
            mensagemErro = client.MessageStatus
        });
    }


    public async Task<List<Client>> GetAllInvoices3Days()
    {
        string query = @"SELECT tblcontas.titularconta,
		                   tblcontrato.whatsappRecebeConta,
		                   tblcontas.rateioReferenciaMes,
		                   tblcontas.rateioReferenciaAno,
		                   tblcontas.datavencimento,
		                   tblcontas.idconta,
		                   tblcontas.rateioTotalroi,
                           tblcontas.rateioEconomia,
                           tblcontas.percentualDescontoTarifaDistribuidora,
                           tblcontas.timestamp,
                           tblcontas.MetaEnvio3Dias,
                           tblcontas.GeracaoMeta,
                           tblcontas.MEtaEnvioVencimento,
                           tblcontas.tipoInstalacao,
                           tblcontas.demandaHFPInjetada,
                           tblboleto.statusBoleto,
                           tblcontrato.flagWppGeracao,
                           tblcontrato.flagWpp7d,
                           tblcontrato.flagWpp3d,
                           tblcontrato.flagWppVencimento,
                           tblcontrato.flagWppInadimplente,
                           tblcontrato.flagWppEstrangeiro
                               FROM tblcontas
                                   INNER JOIN tblcontrato ON tblcontrato.idcontrato = tblcontas.idcontrato
                                   INNER JOIN tblboleto ON tblboleto.idFatura = tblcontas.idconta
                               WHERE tblcontas.preFatura = 'N'
                                 AND tblcontas.rateioTotalroi > 0
                                 AND tblcontrato.getwaypgto <> 'U'
                                 AND tblcontas.flagStatusBoleto = 'Conciliado'
                       	         AND tblcontrato.statuscontrato IN ('Contrato assinado', 'Conectado a Operadora', 'Em processo de Conexão', 'Medidor Zerado', 'Saldo de geração em excesso', 'Desconexão solicitada', 'Inadimplente')
                                 AND tblcontas.StatusContaUsina = 'Aguardando Pagamento'
                                 AND tblboleto.statusBoleto = 'Em Aberto'
                                 AND tblcontas.GeracaoMeta = 'S'
                                 AND tblcontas.MetaEnvio3Dias  = 'N'
                                AND tblcontrato.processoJudicial = 'N'
                                 AND DATEDIFF(tblcontas.datavencimento, CURRENT_DATE())  = 3
                                 Order By tblcontas.idconta DESC ";

        using var connection = _dbConnection.CreateConnection();
        var clients = await connection.QueryAsync<ClientDto>(query);
     
        return clients.Select(ClientMap.MapToClient).ToList();
    }

    public async Task<List<Client>> GetAllInvoicesGenerate()
    {
        string query = @"SELECT tblcontas.titularconta,
		                   tblcontrato.whatsappRecebeConta,
		                   tblcontas.rateioReferenciaMes,
		                   tblcontas.rateioReferenciaAno,
		                   tblcontas.datavencimento,
		                   tblcontas.idconta,
		                   tblcontas.rateioTotalroi,
                           tblcontas.rateioEconomia,
                           tblcontas.percentualDescontoTarifaDistribuidora,
                           tblcontas.timestamp,
                           tblcontas.MetaEnvio3Dias,
                           tblcontas.GeracaoMeta,
                           tblcontas.MEtaEnvioVencimento,
                           tblcontas.tipoInstalacao,
                           tblcontas.demandaHFPInjetada,
                           tblboleto.statusBoleto,
                           tblcontrato.flagWppGeracao,
                           tblcontrato.flagWpp7d,
                           tblcontrato.flagWpp3d,
                           tblcontrato.flagWppVencimento,
                           tblcontrato.flagWppInadimplente,
                           tblcontrato.flagWppEstrangeiro
                               FROM tblcontas
                                   INNER JOIN tblcontrato ON tblcontrato.idcontrato = tblcontas.idcontrato
                                   INNER JOIN tblboleto ON tblboleto.idFatura = tblcontas.idconta
                               WHERE tblcontas.preFatura = 'N'
                                 AND tblcontas.rateioTotalroi > 0
                                 AND tblcontrato.getwaypgto <> 'U'
                                 AND tblcontas.flagStatusBoleto = 'Conciliado'
                       	         AND tblcontrato.statuscontrato IN ('Contrato assinado', 'Conectado a Operadora', 'Em processo de Conexão', 'Medidor Zerado', 'Saldo de geração em excesso', 'Desconexão solicitada', 'Inadimplente')
                                 AND tblcontas.StatusContaUsina = 'Aguardando Pagamento'
                                 AND tblboleto.statusBoleto = 'Em Aberto'
                                 AND tblcontas.GeracaoMeta = 'N'
                                 AND tblcontas.MetaEnvio3Dias  = 'N'
                                 AND tblcontrato.flagWppGeracao = 'S'
                                AND tblcontrato.processoJudicial = 'N'
                                 AND tblcontas.datavencimento  > CURRENT_DATE()
                                 Order By tblcontas.idconta DESC ";

        using var connection = _dbConnection.CreateConnection();
        var clientDtos = await connection.QueryAsync<ClientDto>(query);
        return clientDtos.Select(ClientMap.MapToClient).ToList();
    }

    public async Task<List<Client>> GetAllInvoicesToDueDate()
    {
        string query = @"   SELECT tblcontas.titularconta,
		                   tblcontrato.whatsappRecebeConta,
		                   tblcontas.rateioReferenciaMes,
		                   tblcontas.rateioReferenciaAno,
		                   tblcontas.datavencimento,
		                   tblcontas.idconta,
		                   tblcontas.rateioTotalroi,
                           tblcontas.rateioEconomia,
                           tblcontas.percentualDescontoTarifaDistribuidora,
                           tblcontas.timestamp,
                           tblcontas.MetaEnvio3Dias,
                           tblcontas.GeracaoMeta,
                           tblcontas.tipoInstalacao,
                           tblcontas.demandaHFPInjetada,
                           tblcontas.MetaEnvio3Dias ,
                           tblcontas.GeracaoMeta ,
                           tblcontas.MEtaEnvioVencimento,
                           tblboleto.statusBoleto,
                           tblcontrato.flagWppGeracao,
                           tblcontrato.flagWpp7d,
                           tblcontrato.flagWpp3d,
                           tblcontrato.flagWppVencimento,
                           tblcontrato.flagWppInadimplente,
                           tblcontrato.flagWppEstrangeiro
                               FROM tblcontas
                                   INNER JOIN tblcontrato ON tblcontrato.idcontrato = tblcontas.idcontrato
                                   INNER JOIN tblboleto ON tblboleto.idFatura = tblcontas.idconta
                               WHERE tblcontas.preFatura = 'N'
                                 AND tblcontas.rateioTotalroi > 0
                                 AND tblcontrato.getwaypgto <> 'U'
                                 AND tblcontas.flagStatusBoleto = 'Conciliado'
                       	         AND tblcontrato.statuscontrato IN ('Contrato assinado', 'Conectado a Operadora', 'Em processo de Conexão', 'Medidor Zerado', 'Saldo de geração em excesso', 'Desconexão solicitada', 'Inadimplente')
                                 AND tblcontas.StatusContaUsina IN ('Aguardando Pagamento')
                                 AND tblboleto.statusBoleto IN ('Em Aberto') 
                                 AND tblcontrato.flagWppVencimento = 'S'
                                 AND tblcontas.MEtaEnvioVencimento  = 'N'
								 AND tblcontas.datavencimento = CURRENT_DATE()
                                AND tblcontrato.processoJudicial = 'N'
                                 Order By tblcontas.idconta DESC";

        using var connection = _dbConnection.CreateConnection();
        var clients = await connection.QueryAsync<ClientDto>(query);
 
        return  clients.Select(ClientMap.MapToClient).ToList();
    }

    public async Task<string> GetBoletoBase64ById(int id)
    {
        string query = $@"SELECT pdfBoleto 
                            FROM tblboleto 
                            WHERE idFatura = @idConta
                            AND statusBoleto IN ('Em Aberto', 'Vencida')
                            ORDER BY idboleto DESC";
        using var connection = _dbConnection.CreateConnection();
        var pdfBase64 = await connection.QueryAsync<string>(query, new { idConta = id });
        return pdfBase64.FirstOrDefault();

    }

    public async Task Update3DaysAsync(Client client)
    {
        string send3Days = client.Status == InvoicesEnun.DueDate3 ? "S" : "N";

        string query = $@"UPDATE tblcontas 
                           SET tblcontas.MetaEnvio3Dias = @meta3dias, 
                               tblcontas.dataEnvioWpp3d = @dataEnvio
                           WHERE tblcontas.idconta = @idConta";

        using var connection = _dbConnection.CreateConnection();
        var result = await connection.ExecuteAsync(query, new
        {
            idConta = client.IdConta,
            meta3dias = send3Days,
            dataEnvio = DateTime.Now
        });
    }

    public async Task UpdateAlert(Client client)
    {
        string query = $@"UPDATE tblcontas 
                           SET tblcontas.flagWppAlert = 'S', 
                               tblcontas.dataEnvioWppAlert = @dataEnvio
                           WHERE tblcontas.idconta = @idConta";
        using var connection = _dbConnection.CreateConnection();
        var result = await connection.ExecuteAsync(query, new
        {
            idConta = client.IdConta,
            dataEnvio = DateTime.Now
        });
    }

    public async Task UpdateDueDateAsync(Client client)
    {
        string sendDueDate = client.Status == InvoicesEnun.DueDate0 ? "S" : "N";

        string query = $@"UPDATE tblcontas 
                           SET tblcontas.MEtaEnvioVencimento = @metaEnvioVencimento, 
                               tblcontas.dataEnvioWpp = @dataEnvio
                           WHERE tblcontas.idconta = @idConta";

        using var connection = _dbConnection.CreateConnection();
        var result = await connection.ExecuteAsync(query, new
        {
            idConta = client.IdConta,
            metaEnvioVencimento = sendDueDate,
            dataEnvio = DateTime.Now
        });
    }

    public async Task UpdateGenerateAsync(Client client)
    {
        string sendGeneration = client.Status == InvoicesEnun.Generate ? "S" : "N";

        string query = $@"UPDATE tblcontas 
                           SET tblcontas.GeracaoMeta = @metaGeracao, 
                               tblcontas.dataEnvioWpp = @dataEnvio
                           WHERE tblcontas.idconta = @idConta";

        using var connection = _dbConnection.CreateConnection();
        var result = await connection.ExecuteAsync(query, new
        {
            idConta = client.IdConta,
            metaGeracao = sendGeneration,
            dataEnvio = DateTime.Now
        });
    }
}
