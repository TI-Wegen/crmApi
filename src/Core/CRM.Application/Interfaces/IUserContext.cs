namespace CRM.Application.Interfaces
{
    public interface IUserContext
    {
        Guid? GetCurrentUserId();
    }
}