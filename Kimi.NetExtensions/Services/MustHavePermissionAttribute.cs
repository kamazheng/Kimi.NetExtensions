using Microsoft.AspNetCore.Authorization;

namespace Kimi.NetExtensions.Services;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string resourceCategory, string resource, string action) =>
        Policy = ResourcePermission.NameFor(resourceCategory, resource, action);
}