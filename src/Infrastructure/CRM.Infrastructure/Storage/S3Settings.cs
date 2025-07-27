namespace CRM.Infrastructure.Storage;

public class S3Settings
{
    public string BucketName { get; set; }
    public string AccessKey { get; set; } // Renomeado de Key para mais clareza
    public string SecretKey { get; set; }
    public string Region { get; set; } 
}