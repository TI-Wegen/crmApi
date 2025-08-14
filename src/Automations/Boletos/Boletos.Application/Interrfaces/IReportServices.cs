namespace Boletos.Application.Interrfaces;

public interface IReportServices
{
    Task<string> GetReport(string url, string fileName);
    Task DeleteReport(string fileName);
}
