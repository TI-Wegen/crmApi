namespace CRM.Infrastructure.Storage;

// Em Infrastructure/FileStorage/
using Amazon.S3;
using Amazon.S3.Model;
using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Options;

public class MinioStorageService : IFileStorageService
{
    private readonly MinioSettings _settings;
    private readonly AmazonS3Client _s3Client;

    public MinioStorageService(IOptions<MinioSettings> settings)
    {
        _settings = settings.Value;
        // Configuração para usar um serviço compatível com S3 (como o Minio)
        var config = new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl,
            ForcePathStyle = true // ESSENCIAL para o Minio
        };
        _s3Client = new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, config);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileName, // Nome do arquivo no bucket
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead // Torna o objeto publicamente legível
        };

        await _s3Client.PutObjectAsync(request);

        // Retorna a URL pública do arquivo
        return $"{_settings.ServiceUrl}/{_settings.BucketName}/{fileName}";
    }
}

// Classe para carregar as configurações do appsettings.json
public class MinioSettings
{
    public string ServiceUrl { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string BucketName { get; set; }
}