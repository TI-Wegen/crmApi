using CRM.Domain.Enums;

namespace CRM.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, BucketTypeEnum? type = BucketTypeEnum.CRM);

    Task DeleteFileAsync(string fileName, BucketTypeEnum? type = BucketTypeEnum.CRM);
}
