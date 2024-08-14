namespace Kimi.NetExtensions.Services;

public static class UrlRewriteService
{
    private static List<string> _fileExtensions;
    private static string _newParaUrl;

    static UrlRewriteService()
    {
        _fileExtensions = new List<string> { ".stp", ".step", ".iges", ".3dm", ".3ds", ".wrl" };
        _newParaUrl = @"https://mlxcduvwqapp01.molex.com:50181/GTCM/Online3dViewer/website/#model={0}";
    }

    public static void Init(List<string>? fileExtensions, string? newParaUrl)
    {
        _fileExtensions = fileExtensions ?? new List<string> { ".stp", ".step", ".iges", ".3dm", ".3ds", ".wrl" };
        _newParaUrl = newParaUrl ?? @"https://mlxcduvwqapp01.molex.com:50181/GTCM/Online3dViewer/website/#model={0}";
    }

    public static string UrlRewrite(string fullUrl)
    {
        if (IsFileExtensionInList(fullUrl))
        {
            return string.Format(_newParaUrl, fullUrl);
        }
        else { return fullUrl; }
    }

    private static bool IsFileExtensionInList(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        foreach (var extension in _fileExtensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}