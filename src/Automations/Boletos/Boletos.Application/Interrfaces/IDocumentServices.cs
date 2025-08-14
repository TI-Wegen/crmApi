namespace Boletos.Application.Interrfaces;

public interface IDocumentServices
{
    Task<MemoryStream> MergeDocument(string base64, string path);
}
