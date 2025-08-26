using System.Security.Claims;
using CRM.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CRM.API.Services;

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
