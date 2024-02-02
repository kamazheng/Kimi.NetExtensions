using Microsoft.AspNetCore.Mvc;

namespace Kimi.NetExtensions.Extensions;

public static class OtherExtensions
{
    public static bool IsSuccessCode(this ObjectResult result)
    {
        return result.StatusCode >= 200 && result.StatusCode < 300;
    }
}

