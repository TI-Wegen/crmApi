using Boletos.Application.Interrfaces;

namespace Infrastructure.ExternalServices.Services.Documents;

public class ReportService : IReportServices
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api-pdf-go.onrender.com";

    public ReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DeleteReport(string fileName)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var pathTemp = Path.Combine(projectRoot, "Temp");
        var fullPath = Path.Combine(pathTemp, fileName);
            if (File.Exists(fullPath))
            File.Delete(fullPath);
    
        return;
    }

    public async Task<string> GetReport(string url, string fileName)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao acessar a URL: {(int)response.StatusCode} - {response.ReasonPhrase}");
            
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var pathTemp = Path.Combine(projectRoot, "Temp");
            Directory.CreateDirectory(pathTemp);
            var fullPath = Path.Combine(pathTemp, fileName);
            var requestBody = new
            {
                url = url
            };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var report = await _httpClient.PostAsync($"{_baseUrl}/CreatePdfFromUrl", content);

            if (!report.IsSuccessStatusCode)
                throw new Exception($"Erro ao gerar o relatório: {(int)report.StatusCode} - {report.ReasonPhrase}");


            var reportBytes = await report.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(fullPath, reportBytes);
            return fullPath;

        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao gerar o relatório: {ex.Message}", ex);
        }

    }
}

