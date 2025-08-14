using Boletos.Application.Interrfaces;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
namespace Infrastructure.ExternalServices.Services.Documents;

public class MergeService : IDocumentServices
{
    public async  Task<MemoryStream> MergeDocument(string base64, string path)
    {
        try
        {
            byte[] base64Bytes = Convert.FromBase64String(base64);
            using var stream1 = new MemoryStream(base64Bytes);

            using var stream2 = File.OpenRead(path);

            var outputDocument = new PdfDocument();

            var inputDoc1 = PdfReader.Open(stream1, PdfDocumentOpenMode.Import);
            foreach (var page in inputDoc1.Pages)
            {
                outputDocument.AddPage(page);
            }

            var inputDoc2 = PdfReader.Open(stream2, PdfDocumentOpenMode.Import);
            foreach (var page in inputDoc2.Pages)
            {
                outputDocument.AddPage(page);
            }

            var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);
            outputStream.Position = 0;

            return outputStream;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao mesclar documentos: {ex.Message}", ex);
        }
    }
}

