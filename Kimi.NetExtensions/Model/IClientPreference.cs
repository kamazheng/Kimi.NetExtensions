namespace Kimi.NetExtensions.Model;

public record LanguageCode(string Code, string DisplayName, bool IsRTL = false);

public interface IClientPreference
{
    string? ABColor { get; set; }
    string Font { get; set; }
    int BorderRadius { get; set; }
    float BorderWidth { get; set; }
    bool IsDarkMode { get; set; }
    LanguageCode LanguageCode { get; set; }
    string? NBColor { get; set; }
}