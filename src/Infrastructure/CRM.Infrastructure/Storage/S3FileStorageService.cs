namespace CRM.Infrastructure.Storage;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using global::CRM.Application.Interfaces;
using global::CRM.Domain.Enums;
using Microsoft.Extensions.Options; 
using System.IO;
using System.Threading.Tasks;


public class S3FileStorageService : IFileStorageService
{
    private readonly S3Settings _settings;
    private readonly AmazonS3Client _s3ClientCRM;
    private readonly AmazonS3Client _s3ClientAutomations;

    public S3FileStorageService(IOptions<S3Settings> settings)
    {
        _settings = settings.Value;

        var credentials = new Amazon.Runtime.BasicAWSCredentials(_settings.CRM.AccessKey, _settings.CRM.SecretKey);
        var region = RegionEndpoint.GetBySystemName(_settings.CRM.Region);

        var credentialAutomations = new Amazon.Runtime.BasicAWSCredentials(_settings.Automations.AccessKey, _settings.Automations.SecretKey);

        _s3ClientCRM = new AmazonS3Client(credentials, region);
        _s3ClientAutomations = new AmazonS3Client(credentialAutomations, region);
    }

    public async Task DeleteFileAsync(string fileName, BucketTypeEnum?  type = BucketTypeEnum.CRM)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _settings.CRM.BucketName,
            Key = fileName
        };
        if (type == BucketTypeEnum.Automations)
            deleteObjectRequest.BucketName = _settings.Automations.BucketName;

        try
            {
            var response = await _s3ClientCRM.DeleteObjectAsync(deleteObjectRequest);

            return;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro ao deletar arquivo do S3: {e.Message}");
            throw new ApplicationException("Erro no serviço de storage.", e);

        }
    }

    public async Task<string> UploadAsync(Stream fileStream, 
        string fileName,
        string contentType, 
        BucketTypeEnum? type = BucketTypeEnum.CRM)
    {
        var fileTransferUtility = new TransferUtility(_s3ClientCRM);
        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = fileName,
            BucketName = _settings.CRM.BucketName,
            ContentType = contentType,
        };
        string bucketUrl = $"https://{_settings.CRM.BucketName}.s3.{_settings.CRM.Region}.amazonaws.com/{fileName}";


        if (type == BucketTypeEnum.Automations)
        {
            fileTransferUtility = new TransferUtility(_s3ClientAutomations);
            uploadRequest.BucketName = _settings.Automations.BucketName;
            bucketUrl = $"https://{_settings.Automations.BucketName}.s3.{_settings.Automations.Region}.amazonaws.com/{fileName}";
        }
        try
        {
            await fileTransferUtility.UploadAsync(uploadRequest);

            return bucketUrl;
        }
        catch (Exception e)
        {
         
            Console.WriteLine($"Erro ao fazer upload para o S3: {e.Message}");
            throw new ApplicationException("Erro no serviço de storage.", e);
        }

    }
}