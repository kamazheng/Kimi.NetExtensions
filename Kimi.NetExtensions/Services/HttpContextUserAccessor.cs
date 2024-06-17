using Kimi.NetExtensions.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Kimi.NetExtensions.Services;

public interface IUserAccessor
{
    IUser User { get; }
}

public class HttpContextUserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IUser User => _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(IUser)) as IUser;
}