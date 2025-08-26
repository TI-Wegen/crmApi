namespace Contacts.Application.Abstractions
{
    public interface IMetaContactService
    {
        Task<string?> GetProfilePictureUrlAsync(string waId);
        Task<string?> VerifyContactAndGetWaIdAsync(string phoneNumber);
    }
}