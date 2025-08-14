using Microsoft.Extensions.Configuration;
using System.Data;
using MySqlConnector;


namespace Infrastructure.ExternalServices.DataBase;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ExternalConnection")
            ?? throw new ArgumentException("Connection string not found");
    }
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}

