namespace CRM.Infrastructure.Storage;

public class S3Settings
{
    public CRM CRM { get; set; }
    public Automations Automations { get; set; }

}


public class CRM {
    public string BucketName { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Region { get; set; }
}

public class Automations
{
    public string BucketName { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string Region { get; set; }

}