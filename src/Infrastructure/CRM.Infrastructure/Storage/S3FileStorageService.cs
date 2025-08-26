using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
    private readonly S3Settings _settings;
    private readonly AmazonS3Client _s3Client;

    public S3FileStorageService(IOptions<S3Settings> settings)
    {
        _settings = settings.Value;

        var credentials = new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
        var region = RegionEndpoint.GetBySystemName(_settings.Region);

        _s3Client = new AmazonS3Client(credentials, region);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);

        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = fileName,
                BucketName = _settings.BucketName,
                ContentType = contentType,
            };

            await fileTransferUtility.UploadAsync(uploadRequest);

            string bucketUrl = $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{fileName}";
            return bucketUrl;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro ao fazer upload para o S3: {e.Message}");
            throw new ApplicationException("Erro no serviço de storage.", e);
        }
    }
}