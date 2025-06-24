namespace Conversations.Application.Abstractions
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    }

}
