using System.Data;

namespace Infrastructure.ExternalServices.DataBase
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();

    }


}
