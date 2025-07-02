namespace CRM.API.Services;

using CRM.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (Guid.TryParse(userIdClaim , out var userId))
        {
            return userId;
        }

        return null;
    }
}
