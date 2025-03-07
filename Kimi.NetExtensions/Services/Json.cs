using Newtonsoft.Json;

public static class Json
{
    static Json() {  }

    public static void SetOptions()
    {
        JsonConvert.DefaultSettings = () => ConfigureSettings(new JsonSerializerSettings());
    }

    public static JsonSerializerSettings JsonSerializerSettings()
    {
        return ConfigureSettings(new JsonSerializerSettings());
    }

    public static JsonSerializerSettings ConfigureSettings(JsonSerializerSettings settings, bool ignoreNavigation = false)
    {
        settings.Formatting = Formatting.Indented;
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //settings.TypeNameHandling = TypeNameHandling.Objects;
        if (ignoreNavigation)
        {
            settings.ContractResolver = new JsonIgnoreNavigationResolver();
        }
        return settings;
    }
}