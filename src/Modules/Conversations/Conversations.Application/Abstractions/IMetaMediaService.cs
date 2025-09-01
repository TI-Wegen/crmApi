namespace Conversations.Application.Abstractions;

public record MediaFile(Stream Content, string MimeType, string FileName);

public interface IMetaMediaService
{
    Task<MediaFile?> DownloadMediaAsync(string mediaId);
}