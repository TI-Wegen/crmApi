namespace CRM.Infrastructure.Storage;

public class S3Settings
{
    public string BucketName { get; set; }
    public string AccessKey { get; set; } 
    public string SecretKey { get; set; }
    public string Region { get; set; } 
}