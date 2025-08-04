using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;

namespace Infrastructure.ExternalServices.Services;


public class BoletoService : IBoletoService
{
    private readonly string _connectionString;

    public BoletoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ExternalConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'BoletoConnection' não foi encontrada.");
    }

    public async Task<BoletoDto?> GetBoletoAsync(int idConta)
    {
        try
        {
            const string sql = @"
         	    SELECT 
                    tblboleto.idFatura, 
                    tblboleto.dataVencimento, 
                    tblboleto.pdfboleto,
                    tblboleto.statusBoleto
                FROM 
                    tblboleto
                WHERE   tblboleto.Idfatura = @idFatura
                AND tblboleto.statusBoleto IN ('Em Aberto', 'Vencida')
                ORDER BY tblboleto.idBoleto DESC LIMIT 1;";

            await using var connection = new MySqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<BoletoDto>(sql, new { idFatura = idConta });

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("--- ERRO AO EXECUTAR QUERY COM DAPPER ---");
            Console.WriteLine(e.ToString());
            Console.WriteLine("--- FIM DO ERRO ---");

            throw; 
        }
    }

    public async Task<IEnumerable<BoletoDto>> GetBoletosAbertosAsync(string cpf)
    {
        const string sql = @"
             SELECT 
                tblcontas.Referente, 
                tblcontas.numinstalacao,
                tblboleto.idFatura, 
                tblboleto.dataVencimento, 
                tblboleto.statusBoleto,
                tblboleto.statusBoleto
            FROM tblboleto
            INNER JOIN tblcontas ON tblcontas.idconta = tblboleto.idFatura
            WHERE tblcontas.cpfcnpj = @cpfPagador
              AND tblboleto.statusBoleto IN ('Em Aberto', 'Vencida')
            ORDER BY tblboleto.idBoleto DESC LIMIT 5";

        await using var connection = new MySqlConnection(_connectionString);

        var result = await connection.QueryAsync<BoletoDto>(sql, new { cpfPagador = cpf });
        return result ?? Enumerable.Empty<BoletoDto>();
    }
}