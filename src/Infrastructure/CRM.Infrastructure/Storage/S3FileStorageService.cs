namespace CRM.Infrastructure.Storage;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Conversations.Application.Abstractions;
using Microsoft.Extensions.Options; // Para IOptions
using System.IO;
using System.Threading.Tasks;


// 1. Renomeado e implementando nossa interface
public class S3FileStorageService : IFileStorageService
{
    // 2. Configurações agora são privadas e fortemente tipadas
    private readonly S3Settings _settings;
    private readonly AmazonS3Client _s3Client;

    // 3. Injeta IOptions<S3Settings> em vez de IConfiguration
    public S3FileStorageService(IOptions<S3Settings> settings)
    {
        _settings = settings.Value;

        var credentials = new Amazon.Runtime.BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
        var region = RegionEndpoint.GetBySystemName(_settings.Region);

        // 4. O HttpClient foi removido pois não era usado
        _s3Client = new AmazonS3Client(credentials, region);
    }

    // 5. Assinatura do método agora corresponde à interface
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);

        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName, // O nome do arquivo no S3
                BucketName = _settings.BucketName,
                ContentType = contentType, // 6. ContentType agora é flexível
            };

            await fileTransferUtility.UploadAsync(uploadRequest);

            // 7. URL gerada dinamicamente a partir das configurações
            return $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{fileName}";
        }
        catch (Exception e)
        {
            // Em produção, um log mais detalhado seria ideal aqui.
            // Re-lançar a exceção permite que um middleware global a trate.
            Console.WriteLine($"Erro ao fazer upload para o S3: {e.Message}");
            throw new ApplicationException("Erro no serviço de storage.", e);
        }

    }
}